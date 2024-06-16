using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class FixValveUseCase(IValveRepository valveRepository)
    {
        private readonly IValveRepository valveRepository = valveRepository;

        public void Execute(int valveId)
        {
            Valve? valve = this.valveRepository.Get(valveId);
            if (valve is null)
            {
                return;
            }

            valve = valve with { IsDefective = false };
            this.valveRepository.Save(valve);
        }
    }
}
