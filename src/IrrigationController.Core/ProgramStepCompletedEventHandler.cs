using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class ProgramStepCompletedEventHandler(IIrrigationLog log)
    {
        private readonly IIrrigationLog log = log;

        public void Handle(ProgramStep step)
        {
            this.log.Write($"Valve #{step.ValveId} closed after {step.Duration:mm\\:ss}.");
        }
    }
}
