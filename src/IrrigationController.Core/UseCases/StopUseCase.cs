using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class StopUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public void Execute()
        {
            this.programController.Stop();
        }
    }
}
