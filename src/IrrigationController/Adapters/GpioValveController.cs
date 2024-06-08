using IrrigationController.Core;
using System.Device.Gpio;

namespace IrrigationController.Adapters
{
    public sealed class GpioValveController : IValveController, IDisposable
    {
        private readonly GpioController controller = new();
        private readonly IReadOnlyList<int> valvePins;

        public GpioValveController(IReadOnlyList<int> valvePins)
        {
            this.valvePins = valvePins;
            foreach (int pin in valvePins)
            {
                this.controller.OpenPin(pin, PinMode.Output);
                this.controller.Write(pin, PinValue.Low);
            }
        }

        public int ValveCount => this.valvePins.Count;

        public int? OpenValve { get; private set; }

        public void Open(int valveId)
        {
            for (int i = 0; i < valvePins.Count; i++)
            {
                this.controller.Write(this.valvePins[i], i == valveId ? PinValue.High : PinValue.Low);
            }

            this.OpenValve = valveId;
        }

        public void Close()
        {
            foreach (int pin in this.valvePins)
            {
                this.controller.Write(pin, PinValue.Low);
            }

            this.OpenValve = null;
        }

        public void Dispose()
        {
            this.controller.Dispose();
        }
    }
}
