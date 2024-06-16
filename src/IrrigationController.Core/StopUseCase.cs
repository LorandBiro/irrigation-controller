using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class StopUseCase(ProgramController programController, IIrrigationLog log)
    {
        private readonly ProgramController programController = programController;
        private readonly IIrrigationLog log = log;

        public void Execute()
        {
            ProgramStep? currentStep = this.programController.CurrentStep;
            DateTime? currentStepEndsAt = this.programController.CurrentStepEndsAt;
            if (currentStep is null || currentStepEndsAt is null)
            {
                return;
            }

            this.programController.Stop();

            TimeSpan elapsed = currentStep.Duration - (currentStepEndsAt.Value - DateTime.UtcNow);
            this.log.Write($"Irrigation stopped manually. Valve #{currentStep.ValveId} closed after {elapsed:mm\\:ss}.");
        }
    }
}
