using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class SkipUseCase(ProgramController programController)
{
    public void Execute()
    {
        programController.Skip(IrrigationStopReason.Manual);
    }
}
