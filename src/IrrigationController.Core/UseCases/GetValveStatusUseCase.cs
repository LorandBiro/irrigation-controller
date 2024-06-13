using IrrigationController.Core.Controllers;

namespace IrrigationController.Core.UseCases
{
    public class GetValveStatusUseCase(ValveController valveController)
    {
        private readonly ValveController valveController = valveController;

        public int? OpenValveId => this.valveController.OpenValveId;

        public event EventHandler<int?> OpenValveIdChanged
        {
            add => this.valveController.OpenValveIdChanged += value;
            remove => this.valveController.OpenValveIdChanged -= value;
        }
    }
}
