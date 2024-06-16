using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class ShortCircuitDetectedEventHandler(ProgramController programController, IValveRepository valveRepository)
    {
        private readonly ProgramController programController = programController;
        private readonly IValveRepository valveRepository = valveRepository;

        public void Handle()
        {
            if (this.programController.CurrentStep is null)
            {
                return;
            }

            int valveId = this.programController.CurrentStep.ValveId;
            this.programController.Skip();

            Valve? valve = this.valveRepository.Get(valveId);
            if (valve is null)
            {
                valve = new Valve(valveId, true);
            }
            else
            {
                valve = valve with { IsDefective = true };
            }

            this.valveRepository.Save(valve);
        }
    }
}
