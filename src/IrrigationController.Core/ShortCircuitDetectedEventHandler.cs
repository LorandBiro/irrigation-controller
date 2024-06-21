using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class ShortCircuitDetectedEventHandler(ProgramController programController, IZoneRepository zoneRepository, IIrrigationLog log)
{
    public void Handle()
    {
        ZoneDuration? currentZone = programController.CurrentZone;
        if (currentZone is null)
        {
            return;
        }

        log.Write(new ShortCircuitDetected(DateTime.UtcNow, currentZone.ZoneId));
        programController.Skip(ZoneCloseReason.ShortCircuit);

        Zone? zone = zoneRepository.Get(currentZone.ZoneId);
        if (zone is null)
        {
            zone = new Zone(currentZone.ZoneId, true);
        }
        else
        {
            zone = zone with { IsDefective = true };
        }

        zoneRepository.Save(zone);
    }
}
