using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core;

public class SunriseEventHandler(SunriseEventHandlerConfig config, IRainSensor rainSensor, ProgramController programController, SoilMoistureEstimator soilMoistureEstimator)
{
    public async Task HandleAsync()
    {
        if (rainSensor.IsRaining)
        {
            return;
        }

        List<ZoneDuration> zonesToIrrigate = [];
        for (int i = 0; i < config.Zones.Count; i++)
        {
            (bool enabled, double maxPrecipitation, double irrigationRate) = config.Zones[i];
            if (!enabled)
            {
                continue;
            }

            double soilMoisture = await soilMoistureEstimator.EstimateAsync(i, DateTime.UtcNow);
            double delta = await soilMoistureEstimator.EstimateDeltaAsync(i, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(24));
            if (delta <= 0.0)
            {
                // Negative delta indicates more evapotranspiration than precipitation over the next 24 hours.
                // We should irrigate if soil moisture is forecasted to dry out completely.
                if (soilMoisture + delta <= 0.0)
                {
                    zonesToIrrigate.Add(new ZoneDuration(i, TimeSpan.FromHours((maxPrecipitation - soilMoisture) / irrigationRate)));
                }
            }
            else
            {
                // Positive delta indicates more precipitation than evapotranspiration over the next 24 hours.
                // We should only irrigate if the zone was completely dry the previous day, adjusted for forecasted precipitation.
                if (soilMoisture == 0.0)
                {
                    zonesToIrrigate.Add(new ZoneDuration(i, TimeSpan.FromHours((maxPrecipitation - delta) / irrigationRate)));
                }
            }
        }

        if (zonesToIrrigate.Count == 0)
        {
            return;
        }

        zonesToIrrigate = this.Split(zonesToIrrigate);
        programController.Run(zonesToIrrigate, ZoneOpenReason.Schedule);
    }

    private List<ZoneDuration> Split(List<ZoneDuration> zones)
    {
        if (zones.Count <= 1)
        {
            // We can't split a single zone into multiple parts.
            return zones;
        }

        TimeSpan maxDuration = zones.Max(x => x.Duration);
        if (maxDuration <= config.SplitDuration)
        {
            // No need to split if the longest duration is less than the configured threshold.
            return zones;
        }

        List<ZoneDuration> splitZones = [];
        int rounds = (int)Math.Ceiling(maxDuration.TotalSeconds / config.SplitDuration.TotalSeconds);
        for (int i = 0; i < rounds; i++)
        {
            for (int j = 0; j < zones.Count; j++)
            {
                ZoneDuration zone = zones[j];
                splitZones.Add(new ZoneDuration(zone.ZoneId, TimeSpan.FromSeconds(zone.Duration.TotalSeconds / rounds)));
            }
        }

        return splitZones;
    }
}
