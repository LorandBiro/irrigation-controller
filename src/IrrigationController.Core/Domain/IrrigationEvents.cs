namespace IrrigationController.Core.Domain;

public interface IIrrigationEvent
{
    DateTime Timestamp { get; }
}

public enum IrrigationStartReason
{
    Manual,
    Scheduled,
    LowSoilMoisture,
}

public record IrrigationStarted(DateTime Timestamp, IReadOnlyList<ProgramStep> Steps, IrrigationStartReason Reason) : IIrrigationEvent;

public enum IrrigationStopReason
{
    Manual,
    Rain,
    Completed,
    Schedule,
    ShortCircuit,
    Shutdown,
}

public record IrrigationStopped(DateTime Timestamp, IReadOnlyList<ProgramStep> Steps, IrrigationStopReason Reason) : IIrrigationEvent;

public enum IrrigationSkipReason
{
    Manual,
    ShortCircuit,
}

public record IrrigationSkipped(DateTime Timestamp, ProgramStep Step, IrrigationSkipReason Reason) : IIrrigationEvent;

public record RainDetected(DateTime Timestamp) : IIrrigationEvent;

public record RainCleared(DateTime Timestamp) : IIrrigationEvent;

public record ShortCircuitDetected(DateTime Timestamp, int ZoneId) : IIrrigationEvent;

public record ShortCircuitResolved(DateTime Timestamp, int ZoneId) : IIrrigationEvent;
