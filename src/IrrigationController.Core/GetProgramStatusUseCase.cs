using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class GetProgramStatusUseCase(ProgramController programController, IRainSensor rainSensor, SunriseScheduler sunriseScheduler)
    {
        private readonly ProgramController programController = programController;
        private readonly IRainSensor rainSensor = rainSensor;
        private readonly SunriseScheduler sunriseScheduler = sunriseScheduler;

        public ProgramStep? CurrentStep => programController.CurrentStep;

        public DateTime? CurrentStepEndsAt => programController.CurrentStepEndsAt;

        public IReadOnlyList<ProgramStep> NextSteps => programController.NextSteps;

        public bool IsRaining => this.rainSensor.IsRaining;

        public DateTime NextSunrise => this.sunriseScheduler.NextSunrise;

        public event EventHandler Changed
        {
            add
            {
                this.rainSensor.IsRainingChanged += value;
                this.programController.CurrentStepChanged += value;
                this.sunriseScheduler.NextSunriseChanged += value;
            }

            remove
            {
                this.rainSensor.IsRainingChanged -= value;
                this.programController.CurrentStepChanged -= value;
                this.sunriseScheduler.NextSunriseChanged -= value;
            }
        }
    }
}
