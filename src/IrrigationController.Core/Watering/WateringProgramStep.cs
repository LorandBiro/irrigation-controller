namespace IrrigationController.Watering;

public class WateringProgramStep
{
    public WateringProgramStep(int stationId, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Duration cannot be negative or zero.", nameof(duration));
        }

        this.StationId = stationId;
        this.Duration = duration;
    }

    public int StationId { get; }

    public TimeSpan Duration { get; }
}
