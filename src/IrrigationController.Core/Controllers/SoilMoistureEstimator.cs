using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers;

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

        DateTime endHour = new(t.Year, t.Month, t.Day, t.Hour, 0, 0, DateTimeKind.Utc);
        DateTime startHour = endHour - Range;

        double[] irrigationByHour = this.GetIrrigationByHour(zoneId, startHour, t);
        double[] etByHour = weatherService.GetEToByHour(startHour, t);
        etByHour[^1] *= (t - endHour).TotalHours;

        double moisture = 0.0;
        double cropCoefficient = config.Zones[zoneId].CropCoefficient;
        double precipitationPerRun = config.Zones[zoneId].PrecipitationPerRun;
        for (int i = 0; i < irrigationByHour.Length; i++)
        {
            moisture += irrigationByHour[i] - etByHour[i] * cropCoefficient;
            if (moisture > precipitationPerRun)
            {
                moisture = precipitationPerRun;
            }
            else if (moisture < 0)
            {
                moisture = 0;
            }
        }

        return moisture;
    }

    private double[] GetIrrigationByHour(int zoneId, DateTime startHour, DateTime t)
    {
        int Index(DateTime hour) => (int)Math.Floor((hour - startHour).TotalHours);

        double precipitationRate = config.Zones[zoneId].PrecipitationRate;
        double[] irrigationByHour = new double[(int)Math.Floor((t - startHour).TotalHours + 1)];
        foreach (ZoneClosed e in this.GetZoneClosedEvents(zoneId, startHour, t))
        {
            DateTime opened = e.Timestamp - e.After;
            DateTime openedHour = Trim(opened);
            DateTime closedHour = Trim(e.Timestamp);
            if (openedHour == closedHour)
            {
                irrigationByHour[Index(openedHour)] = e.After.TotalHours * precipitationRate;
            }
            else
            {
                int openedHourIndex = Index(openedHour);
                if (openedHourIndex >= 0)
                {
                    // The index could be negative if the opening of the zone occurred before the start date.
                    irrigationByHour[openedHourIndex] = (1.0 - (opened - openedHour).TotalHours) * precipitationRate;
                }

                int closedHourIndex = Index(closedHour);
                irrigationByHour[closedHourIndex] = (e.Timestamp - closedHour).TotalHours * precipitationRate;
                for (int i = Math.Max(openedHourIndex + 1, 0); i < closedHourIndex; i++)
                {
                    irrigationByHour[i] = precipitationRate;
                }
            }
        }

        ZoneOpened? zoneOpened = this.GetUnclosedZoneOpenedEvent(zoneId, t);
        if (zoneOpened is not null)
        {
            irrigationByHour[irrigationByHour.Length - 1] += (t - zoneOpened.Timestamp).TotalHours * precipitationRate;
        }

        return irrigationByHour;
    }

    private IEnumerable<ZoneClosed> GetZoneClosedEvents(int zoneId, DateTime startHour, DateTime t)
    {
        IReadOnlyList<IIrrigationEvent> all = log.GetAll();
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (all[i].Timestamp > t)
            {
                continue;
            }

            if (all[i].Timestamp < startHour)
            {
                break;
            }

            if (all[i] is ZoneClosed zoneClosed && zoneClosed.ZoneId == zoneId)
            {
                yield return zoneClosed;
            }
        }
    }

    private ZoneOpened? GetUnclosedZoneOpenedEvent(int zoneId, DateTime t)
    {
        IReadOnlyList<IIrrigationEvent> all = log.GetAll();
        DateTime from = t.AddHours(-1.0);
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (all[i].Timestamp > t)
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

    private static DateTime Trim(DateTime t) => new(t.Year, t.Month, t.Day, t.Hour, 0, 0, DateTimeKind.Utc);
}
