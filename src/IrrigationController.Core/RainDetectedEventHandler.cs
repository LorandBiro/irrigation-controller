using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController
{
    public class RainDetectedEventHandler(IIrrigationLog log)
    {
        public void Handle()
        {
            log.Write(new RainDetected(DateTime.UtcNow));
        }
    }
}
