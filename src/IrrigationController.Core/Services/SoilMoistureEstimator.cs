using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Services;

public class SoilMoistureEstimator(IIrrigationLog log, SoilMoistureEstimatorConfig config, IWeatherService weatherService)
{
    public static readonly TimeSpan Range = TimeSpan.FromDays(7);

    public async Task<double> EstimateAsync(int zoneId, DateTime t)
    {
        if (t.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The time must be in UTC.", nameof(t));
        }

        if (zoneId < 0 || zoneId >= config.Zones.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, "The zone id is out of range.");
        }

        DateTime current = t - Range;
        (double maxPrecipitation, double irrigationRate, double cropCoefficient) = config.Zones[zoneId];
        SoilMoistureCalculator calculator = new(current.TrimToHour(), await weatherService.GetRangeAsync(current, t), irrigationRate, maxPrecipitation, cropCoefficient);
        foreach (ZoneClosed e in this.GetZoneClosedEvents(zoneId, current, t).OrderBy(x => x.Timestamp))
        {
            DateTime open = e.Timestamp - e.After;
            if (open < current)
            {
                calculator.Add(current, e.Timestamp, true);
            }
            else
            {
                calculator.Add(current, open, false);
                calculator.Add(open, e.Timestamp, true);
            }

            current = e.Timestamp;
        }

        ZoneOpened? unclosed = this.GetUnclosedZoneOpenedEvent(zoneId, current, t);
        if (unclosed is null)
        {
            calculator.Add(current, t, false);
        }
        else
        {
            calculator.Add(current, unclosed.Timestamp, false);
            calculator.Add(unclosed.Timestamp, t, true);
        }

        return calculator.SoilMoisture;
    }

    private List<ZoneClosed> GetZoneClosedEvents(int zoneId, DateTime from, DateTime to)
    {
        List<ZoneClosed> events = [];
        IReadOnlyList<IIrrigationEvent> all = log.GetAll();
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (all[i].Timestamp > to)
            {
                continue;
            }

            if (all[i].Timestamp < from)
            {
                break;
            }

            if (all[i] is ZoneClosed zoneClosed && zoneClosed.ZoneId == zoneId)
            {
                events.Add(zoneClosed);
            }
        }

        return events;
    }

    private ZoneOpened? GetUnclosedZoneOpenedEvent(int zoneId, DateTime from, DateTime to)
    {
        IReadOnlyList<IIrrigationEvent> all = log.GetAll();
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (all[i].Timestamp > to)
            {
                continue;
            }

            if (all[i].Timestamp < from)
            {
                break;
            }

            if (all[i] is ZoneOpened zoneOpened && zoneOpened.ZoneId == zoneId)
            {
                return zoneOpened;
            }
        }

        return null;
    }

    private class SoilMoistureCalculator(DateTime startHour, WeatherData[] weather, double irrigationRate, double maxPrecipitation, double cropCoefficient)
    {
        public double SoilMoisture { get; private set; }

        public void Add(DateTime from, DateTime to, bool irrigating)
        {
            double fromHour = (from - startHour).TotalHours;
            double toHour = (to - startHour).TotalHours;
            int fromIndex = (int)Math.Floor(fromHour);
            int toIndex = (int)Math.Floor(toHour);
            if (fromIndex == toIndex)
            {
                this.AddHour(fromIndex, irrigating, toHour - fromHour);
            }
            else
            {
                this.AddHour(fromIndex, irrigating, fromIndex + 1.0 - fromHour);
                for (int i = fromIndex + 1; i < toIndex; i++)
                {
                    this.AddHour(i, irrigating, 1.0);
                }

                this.AddHour(toIndex, irrigating, toHour - toIndex);
            }
        }

        private void AddHour(int index, bool irrigating, double duration)
        {
            WeatherData w = weather[index];
            double rate = (w.Precipitation * w.PrecipitationProbability) - (w.ETo * cropCoefficient);
            if (irrigating)
            {
                rate += irrigationRate;
            }

            this.SoilMoisture = Math.Max(0.0, Math.Min(maxPrecipitation, this.SoilMoisture + (rate * duration)));
        }
    }
}
