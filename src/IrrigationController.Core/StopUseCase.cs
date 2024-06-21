using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class StopUseCase(ProgramController programController)
{
    public void Execute()
    {
        programController.Stop(IrrigationStopReason.Manual);
    }
}
