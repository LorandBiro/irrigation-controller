using IrrigationController.Core;
using System.Device.Gpio;

namespace IrrigationController.Adapters
{
    public sealed class Gpio(ILogger<Gpio> logger) : IGpio, IDisposable
    {
        private readonly ILogger<Gpio> logger = logger;
        private readonly GpioController controller = new();

        public void OpenOutput(int pin)
        {
            this.controller.OpenPin(pin, PinMode.Output);
            this.logger.LogDebug("Pin {Pin} opened for output", pin);
        }

        public void Write(int pin, bool value)
        {
            PinValue pinValue = value ? PinValue.High : PinValue.Low;
            this.controller.Write(pin, pinValue);
            this.logger.LogDebug("Pin {Pin} set to {Value}", pin, pinValue);
        }

        public void Dispose()
        {
            this.controller.Dispose();
        }
    }
}
