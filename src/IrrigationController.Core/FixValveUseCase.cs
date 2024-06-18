using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class FixValveUseCase(IValveRepository valveRepository, IIrrigationLog log)
    {
        public void Execute(int valveId)
        {
            Valve? valve = valveRepository.Get(valveId);
            if (valve is null || !valve.IsDefective)
            {
                return;
            }

            valve = valve with { IsDefective = false };
            valveRepository.Save(valve);

            log.Write(new ShortCircuitResolved(DateTime.UtcNow, valveId));
        }
    }
}
