namespace IrrigationController.Core
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
