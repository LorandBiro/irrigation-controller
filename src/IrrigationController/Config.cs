namespace IrrigationController
{
    public class Config
    {
        public required double Latitude { get; init; }
        public required double Longitude { get; init; }
        public required bool MockGpio { get; init; }
        public required string AppDataPath { get; init; }
        public required int RainSensorPin { get; init; }
        public required TimeSpan RainSensorSamplingInterval { get; init; }
        public required int ShortCircuitSensorPin { get; init; }
        public required TimeSpan ValveDelay { get; init; }
        public required TimeSpan ManualLimit { get; init; }
        public required IReadOnlyList<ValveInfo> Valves { get; init; }
    }

    public record ValveInfo(string Name, int Pin);
}
