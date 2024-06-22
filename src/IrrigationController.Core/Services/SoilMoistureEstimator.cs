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

        double Clamp(double moisture) => Math.Max(0.0, Math.Min(config.Zones[zoneId].MaxPrecipitation, moisture));

        double moisture = 0.0;
        DateTime current = t - Range;
        DateTime startHour = current.TrimToHour();
        double precipitationRate = config.Zones[zoneId].PrecipitationRate;
        double[] et = this.GetETByHour(config.Zones[zoneId].CropCoefficient, startHour, t);
        foreach (ZoneClosed e in this.GetZoneClosedEvents(zoneId, startHour, t).OrderBy(x => x.Timestamp))
        {
            DateTime open = e.Timestamp - e.After;
            if (open < current)
            {
                moisture = Clamp(moisture + Sum(et, precipitationRate, 0, (e.Timestamp - startHour).TotalHours));
            }
            else
            {
                moisture = Clamp(moisture + Sum(et, 0, (current - startHour).TotalHours, (open - startHour).TotalHours));
                moisture = Clamp(moisture + Sum(et, precipitationRate, (open - startHour).TotalHours, (e.Timestamp - startHour).TotalHours));
            }

            current = e.Timestamp;
        }

        ZoneOpened? unclosed = this.GetUnclosedZoneOpenedEvent(zoneId, t);
        if (unclosed is null)
        {
            moisture = Clamp(moisture + Sum(et, 0, (current - startHour).TotalHours, (t - startHour).TotalHours));
        }
        else
        {
            moisture = Clamp(moisture + Sum(et, 0, (current - startHour).TotalHours, (unclosed.Timestamp - startHour).TotalHours));
            moisture = Clamp(moisture + Sum(et, precipitationRate, (unclosed.Timestamp - startHour).TotalHours, (t - startHour).TotalHours));
        }

        return moisture / config.Zones[zoneId].MaxPrecipitation;
    }

    private static double Sum(double[] et, double precipitationRate, double from, double to)
    {
        int fromIndex = (int)Math.Floor(from);
        int toIndex = (int)Math.Floor(to);
        if (fromIndex == toIndex)
        {
            return (precipitationRate - et[fromIndex]) * (to - from);
        }
        else
        {
            double sum = (precipitationRate - et[fromIndex]) * (fromIndex + 1 - from);
            for (int i = fromIndex + 1; i < toIndex; i++)
            {
                sum += precipitationRate - et[i];
            }

            sum += (precipitationRate - et[toIndex]) * (to - toIndex);
            return sum;
        }
    }

    private double[] GetETByHour(double cropCoefficient, DateTime from, DateTime to)
    {
        double[] et = weatherService.GetEToByHour(from, to);
        for (int i = 0; i < et.Length; i++)
        {
            et[i] *= cropCoefficient;
        }

        return et;
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

    private ZoneOpened? GetUnclosedZoneOpenedEvent(int zoneId, DateTime to)
    {
        IReadOnlyList<IIrrigationEvent> all = log.GetAll();
        DateTime from = to.AddHours(-1.0);
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
                break;
            }

            if (all[i] is ZoneOpened zoneOpened && zoneOpened.ZoneId == zoneId)
            {
                return zoneOpened;
            }
        }

        return null;
    }
}
