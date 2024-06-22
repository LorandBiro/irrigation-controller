using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Services;

public class SoilMoistureEstimator(IIrrigationLog log, SoilMoistureEstimatorConfig config, IWeatherService weatherService)
{
    public static readonly TimeSpan Range = TimeSpan.FromDays(7);

    public double Estimate(int zoneId, DateTime t)
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
        SoilMoistureCalculator calculator = new(current.TrimToHour(), weatherService.GetEToByHour(current, t), irrigationRate, maxPrecipitation, cropCoefficient);
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

        return calculator.SoilMoisturePercentage;
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

    private class SoilMoistureCalculator(DateTime startHour, double[] eto, double irrigationRate, double maxPrecipitation, double cropCoefficient)
    {
        public double SoilMoisture { get; private set; }

        public double SoilMoisturePercentage => this.SoilMoisture / maxPrecipitation;

        public void Add(DateTime from, DateTime to, bool irrigating)
        {
            double fromHour = (from - startHour).TotalHours;
            double toHour = (to - startHour).TotalHours;
            this.SoilMoisture = Math.Max(0.0, Math.Min(maxPrecipitation, this.SoilMoisture + this.Sum(fromHour, toHour, irrigating)));
        }

        private double Sum(double from, double to, bool irrigating)
        {
            int fromIndex = (int)Math.Floor(from);
            int toIndex = (int)Math.Floor(to);
            if (fromIndex == toIndex)
            {
                return this.Rate(fromIndex, irrigating) * (to - from);
            }
            else
            {
                double sum = this.Rate(fromIndex, irrigating) * (fromIndex + 1 - from);
                for (int i = fromIndex + 1; i < toIndex; i++)
                {
                    sum += this.Rate(i, irrigating);
                }

                sum += this.Rate(toIndex, irrigating) * (to - toIndex);
                return sum;
            }
        }

        private double Rate(int i, bool irrigating)
        {
            double rate = -eto[i] * cropCoefficient;
            if (irrigating)
            {
                rate += irrigationRate;
            }

            return rate;
        }
    }
}
