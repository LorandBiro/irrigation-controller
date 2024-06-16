using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class RunProgramUseCase(ProgramController programController, IValveRepository valveRepository)
    {
        private readonly ProgramController programController = programController;
        private readonly IValveRepository valveRepository = valveRepository;

        public void Execute(Program program)
        {
            foreach (ProgramStep step in program.Steps)
            {
                Valve? valve = this.valveRepository.Get(step.ValveId);
                if (valve?.IsDefective == true)
                {
                    throw new InvalidOperationException($"Can't run program with defective valve #{step.ValveId}");
                }
            }

            this.programController.Run(program);
        }
    }
}
