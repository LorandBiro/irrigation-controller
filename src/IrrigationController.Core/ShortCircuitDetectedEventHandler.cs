using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class ShortCircuitDetectedEventHandler(ProgramController programController, IValveRepository valveRepository)
    {
        private readonly ProgramController programController = programController;
        private readonly IValveRepository valveRepository = valveRepository;

        public void Handle()
        {
            if (programController.CurrentStep is null)
            {
                return;
            }

            int valveId = programController.CurrentStep.ValveId;
            programController.Skip();

            Valve? valve = valveRepository.Get(valveId);
            if (valve is null)
            {
                valve = new Valve(valveId, true);
            }
            else
            {
                valve = valve with { IsDefective = true };
            }

            valveRepository.Save(valve);
        }
    }
}
