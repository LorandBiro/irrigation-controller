using IrrigationController.Core.Controllers;

namespace IrrigationController.Core
{
    public class StopUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public void Execute()
        {
            programController.Stop();
        }
    }
}
