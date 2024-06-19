using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers;

public class ProgramController
{
    private readonly ZoneController zoneController;
    private readonly IIrrigationLog log;

    private readonly Timer timer;
    private readonly List<ZoneDuration> previousZones;
    private readonly List<ZoneDuration> nextZones;

    public ProgramController(ZoneController zoneController, IIrrigationLog log)
    {
        this.zoneController = zoneController;
        this.log = log;

        this.timer = new(this.TimerCallback);
        this.previousZones = [];
        this.nextZones = [];
    }

    public ZoneDuration? CurrentZone { get; private set; }

    public DateTime? CurrentZoneEndsAt { get; private set; }

    public IReadOnlyList<ZoneDuration> NextZones => this.nextZones;

    public event EventHandler? CurrentZoneChanged;

    public void Run(IReadOnlyList<ZoneDuration> zones, IrrigationStartReason reason)
    {
        if (zones.Count == 0)
        {
            throw new ArgumentException("At least one zone must be provided", nameof(zones));
        }

        lock (this.nextZones)
        {
            if (this.CurrentZone is not null && this.CurrentZoneEndsAt is not null)
            {
                this.log.Write(new IrrigationStopped(DateTime.UtcNow, [.. this.previousZones, this.GetAbortedZone()], ToStopReason(reason)));

                this.previousZones.Clear();
                this.nextZones.Clear();
            }

            this.log.Write(new IrrigationStarted(DateTime.UtcNow, zones, reason));

            this.nextZones.AddRange(zones.Skip(1));
            this.CurrentZone = zones[0];
            this.CurrentZoneEndsAt = DateTime.UtcNow + zones[0].Duration;
            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);

            this.zoneController.Open(zones[0].ZoneId);
            this.timer.Change(zones[0].Duration, TimeSpan.Zero);
        }
    }

    public void Skip(IrrigationSkipReason reason)
    {
        lock (this.nextZones)
        {
            if (this.CurrentZone == null)
            {
                return;
            }

            if (this.nextZones.Count == 0)
            {
                this.Stop(ToStopReason(reason));
                return;
            }

            ZoneDuration abortedZone = this.GetAbortedZone();
            this.log.Write(new IrrigationSkipped(DateTime.UtcNow, abortedZone.ZoneId, abortedZone.Duration, reason));

            ZoneDuration nextZone = this.nextZones[0];
            this.previousZones.Add(abortedZone);
            this.nextZones.RemoveAt(0);
            this.CurrentZone = nextZone;
            this.CurrentZoneEndsAt = DateTime.UtcNow + nextZone.Duration;
            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);

            this.zoneController.Open(nextZone.ZoneId);
            this.timer.Change(nextZone.Duration, TimeSpan.Zero);

        }
    }

    public void Stop(IrrigationStopReason reason)
    {
        lock (this.nextZones)
        {
            if (this.CurrentZone is null || this.CurrentZoneEndsAt is null)
            {
                return;
            }

            this.log.Write(new IrrigationStopped(DateTime.UtcNow, [.. this.previousZones, this.GetAbortedZone()], reason));

            this.previousZones.Clear();
            this.nextZones.Clear();
            this.CurrentZone = null;
            this.CurrentZoneEndsAt = null;
            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);

            this.zoneController.Close();
            this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    private void TimerCallback(object? state)
    {
        lock (this.nextZones)
        {
            ZoneDuration currentZone = this.CurrentZone!;
            if (this.nextZones.Count == 0)
            {
                this.log.Write(new IrrigationStopped(DateTime.UtcNow, [.. this.previousZones, currentZone], IrrigationStopReason.Completed));

                this.previousZones.Clear();
                this.nextZones.Clear();
                this.CurrentZone = null;
                this.CurrentZoneEndsAt = null;

                this.zoneController.Close();
                this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
            else
            {
                ZoneDuration nextZone = this.nextZones[0];
                this.previousZones.Add(currentZone);
                this.nextZones.RemoveAt(0);
                this.CurrentZone = nextZone;
                this.CurrentZoneEndsAt = DateTime.UtcNow + nextZone.Duration;

                this.zoneController.Open(nextZone.ZoneId);
                this.timer.Change(nextZone.Duration, Timeout.InfiniteTimeSpan);
            }

            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private ZoneDuration GetAbortedZone()
    {
        return new ZoneDuration(this.CurrentZone!.ZoneId, this.CurrentZone.Duration - (this.CurrentZoneEndsAt!.Value - DateTime.UtcNow));
    }

    private static IrrigationStopReason ToStopReason(IrrigationStartReason reason) => reason switch
    {
        IrrigationStartReason.Manual => IrrigationStopReason.Manual,
        IrrigationStartReason.Scheduled or IrrigationStartReason.LowSoilMoisture => IrrigationStopReason.Schedule,
        _ => throw new ArgumentException("Invalid start reason")
    };

    private static IrrigationStopReason ToStopReason(IrrigationSkipReason reason) => reason switch
    {
        IrrigationSkipReason.Manual => IrrigationStopReason.Manual,
        IrrigationSkipReason.ShortCircuit => IrrigationStopReason.ShortCircuit,
        _ => throw new ArgumentException("Invalid skip reason")
    };
}
