using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController
{
    public class RainClearedEventHandler(IIrrigationLog log)
    {
        public void Handle()
        {
            log.Write(new RainCleared(DateTime.UtcNow));
        }
    }
}
