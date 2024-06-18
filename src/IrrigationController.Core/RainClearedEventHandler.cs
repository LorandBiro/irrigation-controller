using IrrigationController.Core.Domain;

namespace IrrigationController;

public class RainClearedEventHandler(IIrrigationLog log)
{
    public void Handle()
    {
        log.Write(new RainCleared(DateTime.UtcNow));
    }
}
