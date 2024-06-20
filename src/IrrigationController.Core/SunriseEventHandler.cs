using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core;

public class SunriseEventHandler(IRainSensor rainSensor, SunriseEventHandlerConfig config, IIrrigationLog log, ProgramController programController)
{
    // Numbers are in mm/day, based on the following sources:
    // https://pazsitdoktor.hu/pazsitontozes
    // https://hu.wikipedia.org/wiki/Magyarorsz%C3%A1g_%C3%A9ghajlata
    private static readonly double[] EToByMonth = [0, 0, 0, 2.5, 3.5, 3.5, 5, 5, 3.5, 2.5, 0, 0];
    private static readonly TimeSpan HistoryRange = TimeSpan.FromDays(7);

    public void Handle()
    {
        if (rainSensor.IsRaining)
        {
            return;
        }

        Dictionary<(DateOnly Date, int ZoneId), double> irrigationHistory = this.GetIrrigationHistory();
        List<ZoneDuration> zonesToIrrigate = this.GetZonesToIrrigate(irrigationHistory);
        programController.Run(zonesToIrrigate, IrrigationStartReason.FallbackAlgorithm);
    }

    private List<ZoneDuration> GetZonesToIrrigate(Dictionary<(DateOnly Date, int ZoneId), double> irrigationHistory)
    {
        List<ZoneDuration> zonesToIrrigate = [];
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        for (int zoneId = 0; zoneId < config.Zones.Count; zoneId++)
        {
            (bool enabled, double precipitationPerRun, double precipitationRate, double cropCoefficient) = config.Zones[zoneId];
            if (!enabled)
            {
                continue;
            }

            double et = 0.0;
            for (int i = 1; i <= 7; i++)
            {
                DateOnly date = today.AddDays(-i);
                double eto = EToByMonth[date.Month - 1];
                et += eto * cropCoefficient;

                irrigationHistory.TryGetValue((date, zoneId), out double irrigation);
                if (et >= precipitationPerRun)
                {
                    zonesToIrrigate.Add(new ZoneDuration(zoneId, TimeSpan.FromHours(precipitationPerRun / precipitationRate)));
                    break;
                }

                et -= irrigation;
                if (et < 0)
                {
                    break;
                }
            }
        }

        return zonesToIrrigate;
    }

    private Dictionary<(DateOnly Date, int ZoneId), double> GetIrrigationHistory()
    {
        Dictionary<(DateOnly Date, int ZoneId), double> irrigationHistory = new();

        IReadOnlyList<IIrrigationEvent> events = log.GetAll();
        DateTime historyLimit = DateTime.Now - HistoryRange;
        for (int i = events.Count - 1; i >= 0; i--)
        {
            if (events[i] is not IrrigationStopped e)
            {
                continue;
            }

            if (events[i].Timestamp < historyLimit)
            {
                break;
            }

            DateOnly date = DateOnly.FromDateTime(e.Timestamp.ToLocalTime());
            foreach (ZoneDuration zone in e.Zones)
            {
                irrigationHistory.TryGetValue((date, zone.ZoneId), out double irrigation);
                irrigationHistory[(date, zone.ZoneId)] = irrigation + (zone.Duration.TotalHours * config.Zones[zone.ZoneId].PrecipitationRate);
            }
        }

        return irrigationHistory;
    }
}
