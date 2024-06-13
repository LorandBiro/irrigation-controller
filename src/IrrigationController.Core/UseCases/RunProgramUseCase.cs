using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core.UseCases
{
    public class RunProgramUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public void Execute(Program program)
        {
            this.programController.Run(program);
        }
    }
}
