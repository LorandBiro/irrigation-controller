using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class ShortCircuitDetectedEventHandler(ProgramController programController, IValveRepository valveRepository, IIrrigationLog log)
    {
        private readonly ProgramController programController = programController;
        private readonly IValveRepository valveRepository = valveRepository;
        private readonly IIrrigationLog log = log;

        public void Handle()
        {
            ProgramStep? currentStep = this.programController.CurrentStep;
            DateTime? currentStepEndsAt = this.programController.CurrentStepEndsAt;
            if (currentStep is null || currentStepEndsAt is null)
            {
                return;
            }

            programController.Skip();
            Valve? valve = valveRepository.Get(currentStep.ValveId);
            if (valve is null)
            {
                valve = new Valve(currentStep.ValveId, true);
            }
            else
            {
                valve = valve with { IsDefective = true };
            }

            this.valveRepository.Save(valve);

            TimeSpan elapsed = currentStep.Duration - (currentStepEndsAt.Value - DateTime.UtcNow);
            this.log.Write($"Short-circuit detected. Valve #{currentStep.ValveId} marked as defective and closed after {elapsed:mm\\:ss}.");
        }
    }
}
