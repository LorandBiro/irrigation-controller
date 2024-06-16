using IrrigationController.Core.Controllers;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class GetValveStatusUseCase(ValveController valveController, IValveRepository valveRepository)
    {
        private readonly ValveController valveController = valveController;
        private readonly IValveRepository valveRepository = valveRepository;

        public event EventHandler StatusChanged
        {
            add
            {
                valveController.OpenValveIdChanged += value;
                valveRepository.Changed += value;
            }

            remove
            {
                valveController.OpenValveIdChanged -= value;
                valveRepository.Changed -= value;
            }
        }

        public (int? OpenValveId, List<int> defectiveValves) Execute()
        {
            int? openValveId = valveController.OpenValveId;
            List<int> defectiveValves = valveRepository.GetAll().Where(v => v.IsDefective).Select(v => v.Id).ToList();
            return (openValveId, defectiveValves);
        }
    }
}
