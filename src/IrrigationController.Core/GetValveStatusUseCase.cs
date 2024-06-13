namespace IrrigationController.Core
{
    public class GetValveStatusUseCase(IValveController valveController, IReadOnlyList<ValveConfig> valveConfigs)
    {
        private readonly IValveController valveController = valveController;
        private readonly IReadOnlyList<string> valveNames = valveConfigs.Select(x => x.Name).ToList();

        public (IReadOnlyList<string> ValveNames, int? OpenValve) Execute()
        {
            return (this.valveNames, this.valveController.OpenValve);
        }
    }
}
