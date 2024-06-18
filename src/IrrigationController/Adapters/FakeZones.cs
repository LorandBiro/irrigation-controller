using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters;

public class FakeZones(ILogger<FakeZones> logger) : IZones
{
    private readonly ILogger<FakeZones> logger = logger;

    public void Close(int zoneId)
    {
        this.logger.LogDebug("Zone #{Zone} closed", zoneId + 1);
    }

    public void Open(int zoneId)
    {
        this.logger.LogDebug("Zone #{Zone} opened", zoneId + 1);
    }
}
