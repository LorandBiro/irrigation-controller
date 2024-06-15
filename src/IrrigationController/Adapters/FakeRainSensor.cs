using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters
{
    public class FakeRainSensor : IRainSensor
    {
        public bool IsRaining { get; }

        public event EventHandler IsRainingChanged
        {
            add { }
            remove { }
        }
    }
}
