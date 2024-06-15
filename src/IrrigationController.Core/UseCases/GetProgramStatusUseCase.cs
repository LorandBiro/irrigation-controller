using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core.UseCases
{
    public class GetProgramStatusUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public ProgramStep? CurrentStep => this.programController.CurrentStep;

        public DateTime? CurrentStepEndsAt => this.programController.CurrentStepEndsAt;

        public IReadOnlyList<ProgramStep> NextSteps => this.programController.NextSteps;


        public event EventHandler CurrentStepChanged
        {
            add => this.programController.CurrentStepChanged += value;
            remove => this.programController.CurrentStepChanged -= value;
        }
    }
}
