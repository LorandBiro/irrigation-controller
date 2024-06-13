namespace IrrigationController.Core.Infrastructure
{
    public class ValveConfig
    {
        public required TimeSpan ValveDelay { get; init; }
        public required IReadOnlyList<Valve> Valves { get; init; }
    }

    public record Valve(string Name, int Pin);
}
