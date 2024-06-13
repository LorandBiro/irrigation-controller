using IrrigationController.Core;

namespace IrrigationController.Adapters
{
    public class FakeValveController(ILogger<FakeValveController> logger, IReadOnlyList<ValveConfig> valveConfigs) : IValveController
    {
        private readonly ILogger<FakeValveController> logger = logger;
        private readonly IReadOnlyList<ValveConfig> valveConfigs = valveConfigs;

        public int ValveCount => this.valveConfigs.Count;

        public int? OpenValve { get; private set; }

        public void Open(int valve)
        {
            this.logger.LogInformation("Valve #{Valve} opened ({Name})", valve + 1, this.valveConfigs[valve].Name);
            this.OpenValve = valve;
        }

        public void Close()
        {
            this.logger.LogInformation("All valves closed");
            this.OpenValve = null;
        }
    }
}
