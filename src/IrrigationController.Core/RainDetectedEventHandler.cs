using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;

namespace IrrigationController;

public class RainDetectedEventHandler(IIrrigationLog log, ProgramController programController)
{
    public void Handle()
    {
        log.Write(new RainDetected(DateTime.UtcNow));
        programController.Stop(IrrigationStopReason.Rain);
    }
}
