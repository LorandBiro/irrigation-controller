using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core
{
    public class GetZoneStatusUseCase(ZoneController zoneController, IZoneRepository zoneRepository)
    {
        public event EventHandler StatusChanged
        {
            add
            {
                zoneController.OpenZoneIdChanged += value;
                zoneRepository.Changed += value;
            }

            remove
            {
                zoneController.OpenZoneIdChanged -= value;
                zoneRepository.Changed -= value;
            }
        }

        public (int? OpenZoneId, List<int> DefectiveZones) Execute()
        {
            int? openZoneId = zoneController.OpenZoneId;
            List<int> defectiveZoneIds = zoneRepository.GetAll().Where(v => v.IsDefective).Select(v => v.Id).ToList();
            return (openZoneId, defectiveZoneIds);
        }
    }
}
