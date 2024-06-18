using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class RunProgramUseCase(ProgramController programController, IValveRepository valveRepository)
    {
        public void Execute(IReadOnlyList<ProgramStep> steps)
        {
            foreach (ProgramStep step in steps)
            {
                if (valveRepository.Get(step.ValveId)?.IsDefective == true)
                {
                    throw new InvalidOperationException($"Can't run program with defective valve #{step.ValveId}");
                }
            }

            programController.Run(steps, IrrigationStartReason.Manual);
        }
    }
}
