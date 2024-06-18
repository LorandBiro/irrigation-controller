using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class ResolveShortCircuitUseCase(IZoneRepository zoneRepository, IIrrigationLog log)
{
    public void Execute(int zoneId)
    {
        Zone? zone = zoneRepository.Get(zoneId);
        if (zone is null || !zone.IsDefective)
        {
            return;
        }

        zone = zone with { IsDefective = false };
        zoneRepository.Save(zone);

        log.Write(new ShortCircuitResolved(DateTime.UtcNow, zoneId));
    }
}
