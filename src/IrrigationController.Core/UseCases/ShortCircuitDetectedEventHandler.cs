using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class ShortCircuitDetectedEventHandler(ProgramController programController)
    {
        public void Handle()
        {
            programController.Stop();
        }
    }
}
