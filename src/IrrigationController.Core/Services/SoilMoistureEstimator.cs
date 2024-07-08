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
        SoilMoistureCalculator calculator = new(current.TrimToHour(), await weatherService.GetRangeAsync(current, t), cropCoefficient, 0.0, maxPrecipitation);
        foreach (ZoneClosed e in this.GetZoneClosedEvents(zoneId, current, t).OrderBy(x => x.Timestamp))
        {
            DateTime open = e.Timestamp - e.After;
            if (open < current)
            {
                calculator.Add(current, e.Timestamp, irrigationRate);
            }
            else
            {
                calculator.Add(current, open, 0.0);
                calculator.Add(open, e.Timestamp, irrigationRate);
            }

            current = e.Timestamp;
        }

        ZoneOpened? unclosed = this.GetUnclosedZoneOpenedEvent(zoneId, current, t);
        if (unclosed is null)
        {
            calculator.Add(current, t, 0.0);
        }
        else
        {
            calculator.Add(current, unclosed.Timestamp, 0.0);
            calculator.Add(unclosed.Timestamp, t, irrigationRate);
        }

        return calculator.SoilMoisture;
    }

    public async Task<double> EstimateDeltaAsync(int zoneId, DateTime from, DateTime to)
    {
        if (from.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The time must be in UTC.", nameof(from));
        }

        if (to.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The time must be in UTC.", nameof(to));
        }

        if (zoneId < 0 || zoneId >= config.Zones.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, "The zone id is out of range.");
        }

        if (from > to)
        {
            throw new ArgumentException("The start time must be before the end time.", nameof(from));
        }

        SoilMoistureCalculator calculator = new(from.TrimToHour(), await weatherService.GetRangeAsync(from, to), config.Zones[zoneId].CropCoefficient, null, null);
        calculator.Add(from, to, 0.0);
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

    private class SoilMoistureCalculator(DateTime startHour, WeatherData[] weather, double cropCoefficient, double? min, double? max)
    {
        public double SoilMoisture { get; private set; }

        public void Add(DateTime from, DateTime to, double rate)
        {
            double fromHour = (from - startHour).TotalHours;
            double toHour = (to - startHour).TotalHours;
            int fromIndex = (int)Math.Floor(fromHour);
            int toIndex = (int)Math.Floor(toHour);
            if (fromIndex == toIndex)
            {
                this.AddHour(fromIndex, rate, toHour - fromHour);
            }
            else
            {
                this.AddHour(fromIndex, rate, fromIndex + 1.0 - fromHour);
                for (int i = fromIndex + 1; i < toIndex; i++)
                {
                    this.AddHour(i, rate, 1.0);
                }

                this.AddHour(toIndex, rate, toHour - toIndex);
            }
        }

        private void AddHour(int index, double rate, double duration)
        {
            WeatherData w = weather[index];
            rate += (w.Precipitation * w.PrecipitationProbability) - (w.ETo * cropCoefficient);
            this.SoilMoisture += rate * duration;
            if (min is not null)
            {
                this.SoilMoisture = Math.Max(min.Value, this.SoilMoisture);
            }

            if (max is not null)
            {
                this.SoilMoisture = Math.Min(max.Value, this.SoilMoisture);
            }
        }
    }
}
