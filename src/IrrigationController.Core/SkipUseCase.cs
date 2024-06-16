using IrrigationController.Core.Controllers;

namespace IrrigationController.Core
{
    public class SkipUseCase(ProgramController programController)
    {
        private readonly ProgramController programController = programController;

        public void Execute()
        {
            programController.Skip();
        }
    }
}
