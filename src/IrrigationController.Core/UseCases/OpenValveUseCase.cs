using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class OpenValveUseCase(ProgramController programController, ValveConfig valveConfig)
    {
        private readonly ProgramController programController = programController;
        private readonly ValveConfig valveConfig = valveConfig;

        public void Execute(int valveId)
        {
            this.programController.Run(new Program([new ProgramStep(valveId, this.valveConfig.ManualLimit)]));
        }
    }
}
