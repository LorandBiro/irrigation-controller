using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class CloseValveUseCase(ValveController valveController)
    {
        private readonly ValveController valveController = valveController;

        public void Execute()
        {
            this.valveController.Close();
        }
    }
}
