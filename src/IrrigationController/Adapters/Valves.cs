using IrrigationController.Core.Infrastructure;
using System.Device.Gpio;

namespace IrrigationController.Adapters
{
    public sealed class Valves(ILogger<Valves> logger, ValvesConfig config) : IValves, IDisposable
    {
        private readonly ILogger<Valves> logger = logger;
        private readonly ValvesConfig config = config;
        private readonly GpioController controller = new();

        public void Init()
        {
            foreach (int pin in this.config.Pins)
            {
                this.controller.OpenPin(pin, PinMode.Output);
                this.controller.Write(pin, PinValue.Low);
                this.logger.LogDebug("Pin #{Pin} opened for output", pin);
            }
        }

        public void Open(int valveId)
        {
            Write(valveId, true);
            this.logger.LogDebug("Valve #{Valve} opened", valveId);
        }

        public void Close(int valveId)
        {
            Write(valveId, false);
            this.logger.LogDebug("Valve #{Valve} closed", valveId);
        }

        public void Dispose()
        {
            this.controller.Dispose();
        }

        private void Write(int valveId, bool value)
        {
            int pin = this.config.Pins[valveId];
            PinValue pinValue = value ? PinValue.High : PinValue.Low;
            this.controller.Write(pin, pinValue);
        }
    }
}
