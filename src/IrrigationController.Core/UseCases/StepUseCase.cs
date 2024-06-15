using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class StepUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public void Execute()
        {
            this.programController.Step();
        }
    }
}
