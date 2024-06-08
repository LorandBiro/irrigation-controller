namespace IrrigationController.Model;

public class Station
{
    public int Id { get; }

    public bool Blocked { get; }

    public DateTime LastRunTimestamp { get; }

    public TimeSpan LastRunDuration { get; set; }
}
