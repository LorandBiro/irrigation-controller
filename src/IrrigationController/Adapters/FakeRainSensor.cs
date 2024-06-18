using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters
{
    public class FakeRainSensor(RainDetectedEventHandler rainDetectedEventHandler, RainClearedEventHandler rainClearedEventHandler) : IRainSensor
    {
        public bool IsRaining { get; private set; }

        public event EventHandler? IsRainingChanged;

        public void Toggle()
        {
            this.IsRaining = !this.IsRaining;
            if (this.IsRaining)
            {
                rainDetectedEventHandler.Handle();
            }
            else
            {
                rainClearedEventHandler.Handle();
            }

            this.IsRainingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
