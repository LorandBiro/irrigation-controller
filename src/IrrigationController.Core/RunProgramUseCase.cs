using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class RunProgramUseCase(ProgramController programController, IZoneRepository zoneRepository)
{
    public void Execute(IReadOnlyList<ZoneDuration> zones)
    {
        foreach (ZoneDuration zone in zones)
        {
            if (zoneRepository.Get(zone.ZoneId)?.IsDefective == true)
            {
                throw new InvalidOperationException($"Can't run program with defective zone #{zone.ZoneId}");
            }
        }

        programController.Run(zones, ZoneOpenReason.ManualProgram);
    }
}
