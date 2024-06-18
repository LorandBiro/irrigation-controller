using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core;

public class GetProgramStatusUseCase(ProgramController programController, IRainSensor rainSensor, SunriseScheduler sunriseScheduler)
{
    public ProgramStep? CurrentStep => programController.CurrentStep;

    public DateTime? CurrentStepEndsAt => programController.CurrentStepEndsAt;

    public IReadOnlyList<ProgramStep> NextSteps => programController.NextSteps;

    public bool IsRaining => rainSensor.IsRaining;

    public DateTime NextSunrise => sunriseScheduler.NextSunrise;

    public event EventHandler Changed
    {
        add
        {
            rainSensor.IsRainingChanged += value;
            programController.CurrentStepChanged += value;
            sunriseScheduler.NextSunriseChanged += value;
        }

        remove
        {
            rainSensor.IsRainingChanged -= value;
            programController.CurrentStepChanged -= value;
            sunriseScheduler.NextSunriseChanged -= value;
        }
    }
}
