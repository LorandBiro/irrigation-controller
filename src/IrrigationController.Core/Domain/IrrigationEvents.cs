namespace IrrigationController.Core.Domain;

public interface IIrrigationEvent
{
    DateTime Timestamp { get; }
}

public enum IrrigationStartReason
{
    Manual,
    ManualProgram,
    Algorithm,
    FallbackAlgorithm,
}

public record ZoneOpened(DateTime Timestamp, int ZoneId, TimeSpan For, IrrigationStartReason Reason) : IIrrigationEvent;

public enum IrrigationStopReason
{
    Manual,
    Rain,
    Completed,
    Algorithm,
    ShortCircuit,
    Shutdown,
}

public record ZoneClosed(DateTime Timestamp, int ZoneId, TimeSpan After, IrrigationStopReason Reason) : IIrrigationEvent;

public record RainDetected(DateTime Timestamp) : IIrrigationEvent;

public record RainCleared(DateTime Timestamp) : IIrrigationEvent;

public record ShortCircuitDetected(DateTime Timestamp, int ZoneId) : IIrrigationEvent;

public record ShortCircuitResolved(DateTime Timestamp, int ZoneId) : IIrrigationEvent;
