namespace IrrigationController.Core
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
