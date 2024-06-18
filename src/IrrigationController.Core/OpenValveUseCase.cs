using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class OpenValveUseCase(ProgramController programController, OpenValveUseCaseConfig config, IValveRepository valveRepository)
    {
        public void Execute(int valveId)
        {
            if (valveRepository.Get(valveId)?.IsDefective == true)
            {
                throw new InvalidOperationException($"Can't open defective valve #{valveId}");
            }

            programController.Run([new ProgramStep(valveId, config.Duration)], IrrigationStartReason.Manual);
        }
    }
}
