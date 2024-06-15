using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters
{
    public class FakeValves(ILogger<FakeValves> logger) : IValves
    {
        private readonly ILogger<FakeValves> logger = logger;

        public void Close(int valveId)
        {
            this.logger.LogDebug("Valve #{Valve} closed", valveId);
        }

        public void Open(int valveId)
        {
            this.logger.LogDebug("Valve #{Valve} opened", valveId);
        }
    }
}
