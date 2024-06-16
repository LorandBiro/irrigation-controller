using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class FixValveUseCase(IValveRepository valveRepository)
    {
        private readonly IValveRepository valveRepository = valveRepository;

        public void Execute(int valveId)
        {
            Valve? valve = valveRepository.Get(valveId);
            if (valve is null)
            {
                return;
            }

            valve = valve with { IsDefective = false };
            valveRepository.Save(valve);
        }
    }
}
