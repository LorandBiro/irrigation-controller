using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core
{
    public class ShortCircuitDetectedEventHandler(ProgramController programController, IZoneRepository zoneRepository, IIrrigationLog log)
    {
        public void Handle()
        {
            ProgramStep? currentStep = programController.CurrentStep;
            if (currentStep is null)
            {
                return;
            }

            log.Write(new ShortCircuitDetected(DateTime.UtcNow, currentStep.ZoneId));
            programController.Skip(IrrigationSkipReason.ShortCircuit);

            Zone? zone = zoneRepository.Get(currentStep.ZoneId);
            if (zone is null)
            {
                zone = new Zone(currentStep.ZoneId, true);
            }
            else
            {
                zone = zone with { IsDefective = true };
            }

            zoneRepository.Save(zone);
        }
    }
}
