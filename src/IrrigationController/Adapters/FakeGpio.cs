using IrrigationController.Core;

namespace IrrigationController.Adapters
{
    public class FakeGpio(ILogger<FakeGpio> logger) : IGpio
    {
        private readonly ILogger<FakeGpio> logger = logger;

        public void OpenOutput(int pin)
        {
            this.logger.LogDebug("Pin {Pin} opened for output", pin);
        }

        public void Write(int pin, bool value)
        {
            this.logger.LogDebug("Pin {Pin} set to {Value}", pin, value);
        }
    }
}
