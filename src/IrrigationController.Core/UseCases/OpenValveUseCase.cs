using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class OpenValveUseCase(ValveController valveController)
    {
        private readonly ValveController valveController = valveController;

        public void Execute(int valveId)
        {
            this.valveController.Open(valveId);
        }
    }
}
