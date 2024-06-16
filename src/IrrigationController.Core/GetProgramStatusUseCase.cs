using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class GetProgramStatusUseCase(ProgramController programController, IRainSensor rainSensor)
    {
        private readonly ProgramController programController = programController;
        private readonly IRainSensor rainSensor = rainSensor;

        public ProgramStep? CurrentStep => programController.CurrentStep;

        public DateTime? CurrentStepEndsAt => programController.CurrentStepEndsAt;

        public IReadOnlyList<ProgramStep> NextSteps => programController.NextSteps;

        public event EventHandler CurrentStepChanged
        {
            add => programController.CurrentStepChanged += value;
            remove => programController.CurrentStepChanged -= value;
        }

        public bool IsRaining => rainSensor.IsRaining;

        public event EventHandler IsRainingChanged
        {
            add => rainSensor.IsRainingChanged += value;
            remove => rainSensor.IsRainingChanged -= value;
        }
    }
}
