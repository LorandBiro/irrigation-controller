using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core;

public class GetProgramStatusUseCase(ProgramController programController, IRainSensor rainSensor, SunriseScheduler sunriseScheduler)
{
    public ZoneDuration? CurrentZone => programController.CurrentZone;

    public DateTime? CurrentZoneEndsAt => programController.CurrentZoneEndsAt;

    public IReadOnlyList<ZoneDuration> NextZones => programController.NextZones;

    public bool IsRaining => rainSensor.IsRaining;

    public DateTime NextSunrise => sunriseScheduler.NextSunrise;

    public event EventHandler Changed
    {
        add
        {
            rainSensor.IsRainingChanged += value;
            programController.CurrentZoneChanged += value;
            sunriseScheduler.NextSunriseChanged += value;
        }

        remove
        {
            rainSensor.IsRainingChanged -= value;
            programController.CurrentZoneChanged -= value;
            sunriseScheduler.NextSunriseChanged -= value;
        }
    }
}
