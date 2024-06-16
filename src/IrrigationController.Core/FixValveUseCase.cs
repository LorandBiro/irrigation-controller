using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class FixValveUseCase(IValveRepository valveRepository, IIrrigationLog log)
    {
        private readonly IValveRepository valveRepository = valveRepository;
        private readonly IIrrigationLog log = log;

        public void Execute(int valveId)
        {
            Valve? valve = valveRepository.Get(valveId);
            if (valve is null)
            {
                return;
            }

            valve = valve with { IsDefective = false };
            valveRepository.Save(valve);

            this.log.Write($"Valve #{valveId} maked as fixed.");
        }
    }
}
