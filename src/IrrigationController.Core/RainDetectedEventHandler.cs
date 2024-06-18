using IrrigationController.Core.Domain;

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
