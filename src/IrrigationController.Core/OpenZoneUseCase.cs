using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class OpenZoneUseCase(ProgramController programController, OpenZoneUseCaseConfig config, IZoneRepository zoneRepository)
{
    public void Execute(int zoneId)
    {
        if (zoneRepository.Get(zoneId)?.IsDefective == true)
        {
            throw new InvalidOperationException($"Can't open defective zone #{zoneId}");
        }

        programController.Run([new ZoneDuration(zoneId, config.ManualZoneDuration)], IrrigationStartReason.Manual);
    }
}
