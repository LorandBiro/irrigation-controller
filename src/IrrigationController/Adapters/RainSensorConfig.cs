namespace IrrigationController.Adapters
{
    public record RainSensorConfig(int Pin, TimeSpan SamplingInterval);
}
