using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class GetZoneStatusUseCase(IZoneRepository zoneRepository, ProgramController programController)
{
    public event EventHandler StatusChanged
    {
        add
        {
            programController.CurrentZoneChanged += value;
            zoneRepository.Changed += value;
        }

        remove
        {
            programController.CurrentZoneChanged -= value;
            zoneRepository.Changed -= value;
        }
    }

    public (int? OpenZoneId, List<int> DefectiveZones, bool Controllable) Execute()
    {
        int? openZoneId = programController.CurrentZone?.ZoneId;
        List<int> defectiveZoneIds = zoneRepository.GetAll().Where(v => v.IsDefective).Select(v => v.Id).ToList();
        bool controllable = programController.CurrentZone == null || programController.Reason == IrrigationStartReason.Manual;
        return (openZoneId, defectiveZoneIds, controllable);
    }
}
