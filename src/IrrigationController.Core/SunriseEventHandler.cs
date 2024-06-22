using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core;

public class SunriseEventHandler(IRainSensor rainSensor, SunriseEventHandlerConfig config, ProgramController programController, SoilMoistureEstimator estimator)
{
    public void Handle()
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

            double moisture = estimator.Estimate(i, DateTime.UtcNow);
            if (moisture == 0.0)
            { 
                zonesToIrrigate.Add(new ZoneDuration(i, TimeSpan.FromHours(maxPrecipitation / irrigationRate)));
            }
        }

        programController.Run(zonesToIrrigate, ZoneOpenReason.FallbackSchedule);
    }
}
