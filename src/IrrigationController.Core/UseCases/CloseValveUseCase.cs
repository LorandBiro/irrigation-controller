using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class CloseValveUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public void Execute()
        {
            this.programController.Stop();
        }
    }
}
