using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class OpenZoneUseCase(ProgramController programController, OpenZoneUseCaseConfig config, IZoneRepository zoneRepository)
    {
        public void Execute(int zoneId)
        {
            if (zoneRepository.Get(zoneId)?.IsDefective == true)
            {
                throw new InvalidOperationException($"Can't open defective zone #{zoneId}");
            }

            programController.Run([new ProgramStep(zoneId, config.Duration)], IrrigationStartReason.Manual);
        }
    }
}
