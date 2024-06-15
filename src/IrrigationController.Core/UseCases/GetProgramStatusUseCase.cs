using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class GetProgramStatusUseCase(ProgramController programController, IRainSensor rainSensor)
    {
        private readonly ProgramController programController = programController;
        private readonly IRainSensor rainSensor = rainSensor;

        public ProgramStep? CurrentStep => this.programController.CurrentStep;

        public DateTime? CurrentStepEndsAt => this.programController.CurrentStepEndsAt;

        public IReadOnlyList<ProgramStep> NextSteps => this.programController.NextSteps;


        public event EventHandler CurrentStepChanged
        {
            add => this.programController.CurrentStepChanged += value;
            remove => this.programController.CurrentStepChanged -= value;
        }

        public bool IsRaining => this.rainSensor.IsRaining;

        public event EventHandler IsRainingChanged
        {
            add => this.rainSensor.IsRainingChanged += value;
            remove => this.rainSensor.IsRainingChanged -= value;
        }
    }
}
