using IrrigationController.Core.Controllers;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.UseCases
{
    public class GetValveStatusUseCase(ValveController valveController, IValveRepository valveRepository)
    {
        private readonly ValveController valveController = valveController;
        private readonly IValveRepository valveRepository = valveRepository;

        public event EventHandler StatusChanged
        {
            add
            {
                this.valveController.OpenValveIdChanged += value;
                this.valveRepository.Changed += value;
            }

            remove
            {
                this.valveController.OpenValveIdChanged -= value;
                this.valveRepository.Changed -= value;
            }
        }

        public (int? OpenValveId, List<int> defectiveValves) Execute()
        {
            int? openValveId = this.valveController.OpenValveId;
            List<int> defectiveValves = this.valveRepository.GetAll().Where(v => v.IsDefective).Select(v => v.Id).ToList();
            return (openValveId, defectiveValves);
        }
    }
}
