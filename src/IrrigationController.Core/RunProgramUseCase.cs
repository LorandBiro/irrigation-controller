using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class RunProgramUseCase(ProgramController programController, IZoneRepository zoneRepository)
    {
        public void Execute(IReadOnlyList<ProgramStep> steps)
        {
            foreach (ProgramStep step in steps)
            {
                if (zoneRepository.Get(step.ZoneId)?.IsDefective == true)
                {
                    throw new InvalidOperationException($"Can't run program with defective zone #{step.ZoneId}");
                }
            }

            programController.Run(steps, IrrigationStartReason.Manual);
        }
    }
}
