using IrrigationController.Core;
using System.Diagnostics;

namespace IrrigationController.Adapters
{
    public class FakeValveController(ILogger<FakeValveController> logger) : IValveController
    {
        private readonly ILogger<FakeValveController> logger = logger;

        public int ValveCount => 4;

        public int? OpenValve { get; private set; }

        public void Open(int valveId)
        {
            this.logger.LogInformation($"FakeValveController.Open({valveId})");
            this.OpenValve = valveId;
        }

        public void Close()
        {
            this.logger.LogInformation("FakeValveController.Close");
            this.OpenValve = null;
        }
    }
}
