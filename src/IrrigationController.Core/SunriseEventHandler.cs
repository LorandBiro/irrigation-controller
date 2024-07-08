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
                // We should irrigate if soil moisture is below a threshold to prevent excessive drying.
                // The threshold is 50% of the forecasted decrease in soil moisture.
                // Example: If a 3mm decrease is expected, irrigation is triggered if soil moisture is below 1.5mm.
                double limit = delta * -0.5;
                if (soilMoisture < limit)
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

        programController.Run(zonesToIrrigate, ZoneOpenReason.FallbackSchedule);
    }
}
