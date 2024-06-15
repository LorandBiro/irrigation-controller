using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core.UseCases
{
    public class OpenValveUseCase(ProgramController programController, OpenValveUseCaseConfig config)
    {
        private readonly ProgramController programController = programController;
        private readonly OpenValveUseCaseConfig config = config;

        public void Execute(int valveId)
        {
            this.programController.Run(new Program([new ProgramStep(valveId, this.config.Duration)]));
        }
    }
}
