namespace IrrigationController.Core.Infrastructure
{
    public interface IRainSensor
    {
        bool IsRaining { get; }

        event EventHandler IsRainingChanged;
    }
}
