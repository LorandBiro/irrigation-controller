using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class OpenValveUseCase(ProgramController programController, OpenValveUseCaseConfig config, IValveRepository valveRepository)
    {
        private readonly ProgramController programController = programController;
        private readonly OpenValveUseCaseConfig config = config;
        private readonly IValveRepository valveRepository = valveRepository;

        public void Execute(int valveId)
        {
            Valve? valve = this.valveRepository.Get(valveId);
            if (valve?.IsDefective == true)
            {
                throw new InvalidOperationException($"Can't open defective valve #{valveId}");
            }

            this.programController.Run(new Program([new ProgramStep(valveId, this.config.Duration)]));
        }
    }
}
