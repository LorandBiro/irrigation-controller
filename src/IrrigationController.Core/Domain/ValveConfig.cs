namespace IrrigationController.Core.Domain
{
    public class ValveConfig
    {
        public required TimeSpan ValveDelay { get; init; }
        public required TimeSpan ManualLimit { get; init; }
        public required IReadOnlyList<Valve> Valves { get; init; }
    }

    public record Valve(string Name, int Pin);
}
