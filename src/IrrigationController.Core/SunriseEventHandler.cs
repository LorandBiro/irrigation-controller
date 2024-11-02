using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core;

public class SunriseEventHandler(SunriseEventHandlerConfig config, IRainSensor rainSensor, ProgramController programController, SoilMoistureEstimator soilMoistureEstimator, IWeatherService weatherService, ILog<SunriseEventHandler> log)
{
    public async Task HandleAsync()
    {
        log.Info("Sunrise event handler triggered");
        if (rainSensor.IsRaining)
        {
            log.Info("It's raining, irrigation is skipped");
            return;
        }

        WeatherData currentWeather = await weatherService.GetCurrentAsync();
        if (currentWeather.Temperature < 5.0)
        {
            log.Info($"Temperature is too low ({currentWeather.Temperature:0.0}°C), irrigation is skipped");
            return;
        }

        List<ZoneDuration> zonesToIrrigate = [];
        for (int i = 0; i < config.Zones.Count; i++)
        {
            (bool enabled, double maxPrecipitation, double irrigationRate) = config.Zones[i];
            if (!enabled)
            {
                log.Debug($"Zone #{i} - Disabled");
                continue;
            }

            double soilMoisture = await soilMoistureEstimator.EstimateAsync(i, DateTime.UtcNow);
            double delta = await soilMoistureEstimator.EstimateDeltaAsync(i, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(24));
            string decision;
            if (delta <= 0.0)
            {
                // Negative delta indicates more evapotranspiration than precipitation over the next 24 hours.
                // We should irrigate if soil moisture is forecasted to dry out completely.
                if (soilMoisture + delta > 0.0)
                {
                    decision = "Soil moisture is sufficient for the next day, zone can be skipped";
                }
                else
                {
                    double missing = maxPrecipitation - soilMoisture;
                    zonesToIrrigate.Add(new ZoneDuration(i, TimeSpan.FromHours(missing / irrigationRate)));
                    decision = $"Zone would dry out completely over the next day, it needs {missing:0.00}mm";
                }
            }
            else
            {
                // Positive delta indicates more precipitation than evapotranspiration over the next 24 hours.
                // We should only irrigate if the zone was completely dry the previous day, adjusted for forecasted precipitation.
                if (soilMoisture == 0.0)
                {
                    double missing = maxPrecipitation - delta;
                    zonesToIrrigate.Add(new ZoneDuration(i, TimeSpan.FromHours(missing / irrigationRate)));
                    decision = $"Zone already dried out completely, it needs {missing:0.00}mm";
                }
                else
                {
                    decision = "Sufficient rain is forecasted, zone can be skipped";
                }
            }

            log.Debug($"Zone #{i} - Current soil moisture: {soilMoisture:0.00}mm, 24h forecasted change: {delta:0.00}mm - {decision}");
        }

        if (zonesToIrrigate.Count == 0)
        {
            log.Info("No zone needs irrigation");
            return;
        }

        log.Info("Starting irrigation: " + string.Join(", ", zonesToIrrigate.Select(x => $"#{x.ZoneId} - {x.Duration}")));
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
