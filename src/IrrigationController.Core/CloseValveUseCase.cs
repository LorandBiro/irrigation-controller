namespace IrrigationController.Core
{
    public class CloseValveUseCase(IValveController valveController)
    {
        private readonly IValveController valveController = valveController;

        public void Execute()
        {
            this.valveController.Close();
        }
    }
}
