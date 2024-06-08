namespace IrrigationController.Core
{
    public class GetValveStatusUseCase(IValveController valveController)
    {
        private readonly IValveController valveController = valveController;

        public (int ValveCount, int? OpenValve) Execute()
        {
            return (this.valveController.ValveCount, this.valveController.OpenValve);
        }
    }
}
