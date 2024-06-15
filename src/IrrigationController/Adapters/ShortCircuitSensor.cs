using IrrigationController.Core.UseCases;
using System.Device.Gpio;

namespace IrrigationController.Adapters
{
    public class ShortCircuitSensor(ILogger<ShortCircuitSensor> logger, ShortCircuitSensorConfig config, ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler) : IDisposable
    {
        private readonly ILogger<ShortCircuitSensor> logger = logger;
        private readonly ShortCircuitSensorConfig config = config;
        private readonly ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler = shortCircuitDetectedEventHandler;

        private readonly GpioController gpio = new();

        public void Init()
        {
            this.gpio.OpenPin(this.config.Pin, PinMode.Input);
            this.gpio.RegisterCallbackForPinValueChangedEvent(this.config.Pin, PinEventTypes.Rising, this.OnShortCircuitDetectedCallback);
            this.logger.LogDebug("Short circuit sensor pin {Pin} opened for input. Current state: {State}", this.config.Pin, this.gpio.Read(this.config.Pin));
        }

        public void Dispose()
        {
            this.gpio.Dispose();
        }

        private void OnShortCircuitDetectedCallback(object? sender, PinValueChangedEventArgs e)
        {
            this.logger.LogDebug("Short circuit pin was {Value}", e.ChangeType);
            if (e.ChangeType == PinEventTypes.Rising)
            {
                this.logger.LogCritical("Short circuit detected");
                this.shortCircuitDetectedEventHandler.Handle();
            }
        }
    }
}
