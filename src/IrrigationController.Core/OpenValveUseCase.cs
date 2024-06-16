using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class OpenValveUseCase(ProgramController programController, OpenValveUseCaseConfig config, IValveRepository valveRepository)
    {
        private readonly ProgramController programController = programController;
        private readonly OpenValveUseCaseConfig config = config;
        private readonly IValveRepository valveRepository = valveRepository;

        public void Execute(int valveId)
        {
            Valve? valve = valveRepository.Get(valveId);
            if (valve?.IsDefective == true)
            {
                throw new InvalidOperationException($"Can't open defective valve #{valveId}");
            }

            programController.Run(new Program([new ProgramStep(valveId, config.Duration)]));
        }
    }
}
