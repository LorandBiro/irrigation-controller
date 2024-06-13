using IrrigationController.Core;
using System.Device.Gpio;

namespace IrrigationController.Adapters
{
    public sealed class GpioValveController : IValveController, IDisposable
    {
        private readonly ILogger<GpioValveController> logger;
        private readonly IReadOnlyList<ValveConfig> valveConfigs;

        private readonly GpioController controller = new();

        public GpioValveController(ILogger<GpioValveController> logger, IReadOnlyList<ValveConfig> valveConfigs)
        {
            this.logger = logger;
            this.valveConfigs = valveConfigs;
            foreach (ValveConfig valveConfig in valveConfigs)
            {
                this.controller.OpenPin(valveConfig.Pin, PinMode.Output);
                this.controller.Write(valveConfig.Pin, PinValue.Low);
            }
        }

        public int ValveCount => this.valveConfigs.Count;

        public int? OpenValve { get; private set; }

        public void Open(int valve)
        {
            for (int i = 0; i < valveConfigs.Count; i++)
            {
                this.controller.Write(this.valveConfigs[i].Pin, i == valve ? PinValue.High : PinValue.Low);
            }

            this.logger.LogInformation("Valve #{Valve} opened ({Name})", valve + 1, this.valveConfigs[valve].Name);
            this.OpenValve = valve;
        }

        public void Close()
        {
            foreach (ValveConfig valveConfig in this.valveConfigs)
            {
                this.controller.Write(valveConfig.Pin, PinValue.Low);
            }

            this.logger.LogInformation("All valves closed");
            this.OpenValve = null;
        }

        public void Dispose()
        {
            this.controller.Dispose();
        }
    }
}
