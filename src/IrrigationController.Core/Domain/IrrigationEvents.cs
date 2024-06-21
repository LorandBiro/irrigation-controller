namespace IrrigationController.Core.Domain;

public interface IIrrigationEvent
{
    DateTime Timestamp { get; }
}

public enum ZoneOpenReason
{
    Manual,
    ManualProgram,
    Schedule,
    FallbackSchedule,
}

public record ZoneOpened(DateTime Timestamp, int ZoneId, TimeSpan For, ZoneOpenReason Reason) : IIrrigationEvent;

public enum ZoneCloseReason
{
    Manual,
    Rain,
    Completed,
    Schedule,
    ShortCircuit,
    Shutdown,
}

public record ZoneClosed(DateTime Timestamp, int ZoneId, TimeSpan After, ZoneCloseReason Reason) : IIrrigationEvent;

public record RainDetected(DateTime Timestamp) : IIrrigationEvent;

public record RainCleared(DateTime Timestamp) : IIrrigationEvent;

public record ShortCircuitDetected(DateTime Timestamp, int ZoneId) : IIrrigationEvent;

public record ShortCircuitResolved(DateTime Timestamp, int ZoneId) : IIrrigationEvent;
