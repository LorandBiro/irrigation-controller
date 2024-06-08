namespace IrrigationController.Core
{
    public class OpenValveUseCase(IValveController valveController)
    {
        private readonly IValveController valveController = valveController;

        public void Execute(int valveId)
        {
            this.valveController.Open(valveId);
        }
    }
}
