namespace IrrigationController
{
    public class Config
    {
        public required int RainSensorPin { get; init; }
        public required TimeSpan RainSensorSamplingInterval { get; init; }
        public required int ShortCircuitSensorPin { get; init; }
        public required TimeSpan ValveDelay { get; init; }
        public required TimeSpan ManualLimit { get; init; }
        public required IReadOnlyList<ValveInfo> Valves { get; init; }
    }

    public record ValveInfo(string Name, int Pin);
}
