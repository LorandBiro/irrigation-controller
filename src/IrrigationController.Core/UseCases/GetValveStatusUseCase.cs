using IrrigationController.Core.Controllers;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class GetValveStatusUseCase(ValveController valveController, ValveConfig valveConfig)
    {
        private readonly ValveController valveController = valveController;
        private readonly IReadOnlyList<string> valveNames = valveConfig.Valves.Select(x => x.Name).ToList();

        public (IReadOnlyList<string> ValveNames, int? OpenValveId) Execute()
        {
            return (this.valveNames, this.valveController.OpenValveId);
        }

        public event EventHandler<int?> OpenValveIdChanged
        {
            add => this.valveController.OpenValveIdChanged += value;
            remove => this.valveController.OpenValveIdChanged -= value;
        }
    }
}
