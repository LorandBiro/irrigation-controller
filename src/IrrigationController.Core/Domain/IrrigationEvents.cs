namespace IrrigationController.Core.Domain;

public interface IIrrigationEvent
{
    DateTime Timestamp { get; }
}

public enum IrrigationStartReason
{
    Manual,
    Algorithm,
    FallbackAlgorithm,
}

public record IrrigationStarted(DateTime Timestamp, IReadOnlyList<ZoneDuration> Zones, IrrigationStartReason Reason) : IIrrigationEvent;

public enum IrrigationStopReason
{
    Manual,
    Rain,
    Completed,
    Algorithm,
    ShortCircuit,
    Shutdown,
}

public record IrrigationStopped(DateTime Timestamp, IReadOnlyList<ZoneDuration> Zones, IrrigationStopReason Reason) : IIrrigationEvent;

public enum IrrigationSkipReason
{
    Manual,
    ShortCircuit,
}

public record IrrigationSkipped(DateTime Timestamp, int ZoneId, TimeSpan After, IrrigationSkipReason Reason) : IIrrigationEvent;

public record RainDetected(DateTime Timestamp) : IIrrigationEvent;

public record RainCleared(DateTime Timestamp) : IIrrigationEvent;

public record ShortCircuitDetected(DateTime Timestamp, int ZoneId) : IIrrigationEvent;

public record ShortCircuitResolved(DateTime Timestamp, int ZoneId) : IIrrigationEvent;
