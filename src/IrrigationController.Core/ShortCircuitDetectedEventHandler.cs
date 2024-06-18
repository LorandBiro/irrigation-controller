using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class ShortCircuitDetectedEventHandler(ProgramController programController, IValveRepository valveRepository, IIrrigationLog log)
    {
        public void Handle()
        {
            ProgramStep? currentStep = programController.CurrentStep;
            if (currentStep is null)
            {
                return;
            }

            log.Write(new ShortCircuitDetected(DateTime.UtcNow, currentStep.ValveId));
            programController.Skip(IrrigationSkipReason.ShortCircuit);

            Valve? valve = valveRepository.Get(currentStep.ValveId);
            if (valve is null)
            {
                valve = new Valve(currentStep.ValveId, true);
            }
            else
            {
                valve = valve with { IsDefective = true };
            }

            valveRepository.Save(valve);
        }
    }
}
