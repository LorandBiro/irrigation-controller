using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters
{
    public class FakeRainSensor : IRainSensor
    {
        public bool IsRaining { get; private set; }

        public event EventHandler? IsRainingChanged;

        public void Toggle()
        {
            this.IsRaining = !this.IsRaining;
            this.IsRainingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
