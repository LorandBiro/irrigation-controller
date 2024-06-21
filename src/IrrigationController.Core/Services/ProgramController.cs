using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Services;

public class ProgramController
{
    private readonly ZoneController zoneController;
    private readonly IIrrigationLog log;

    private readonly Timer timer;
    private readonly List<ZoneDuration> nextZones;

    public ProgramController(ZoneController zoneController, IIrrigationLog log)
    {
        this.zoneController = zoneController;
        this.log = log;

        this.timer = new(this.TimerCallback);
        this.nextZones = [];
    }

    public ZoneDuration? CurrentZone { get; private set; }

    public DateTime? CurrentZoneEndsAt { get; private set; }

    public ZoneOpenReason Reason { get; private set; }

    public IReadOnlyList<ZoneDuration> NextZones => this.nextZones;

    public event EventHandler? CurrentZoneChanged;

    public void Run(IReadOnlyList<ZoneDuration> zones, ZoneOpenReason reason)
    {
        if (zones.Count == 0)
        {
            throw new ArgumentException("At least one zone must be provided", nameof(zones));
        }

        lock (this.nextZones)
        {
            if (this.CurrentZone is not null && this.CurrentZoneEndsAt is not null)
            {
                ZoneDuration aborted = this.GetAbortedZone();
                this.log.Write(new ZoneClosed(DateTime.UtcNow, aborted.ZoneId, aborted.Duration, ToStopReason(reason)));

                this.nextZones.Clear();
            }

            ZoneDuration first = zones[0];
            this.log.Write(new ZoneOpened(DateTime.UtcNow, first.ZoneId, first.Duration, reason));

            this.nextZones.AddRange(zones.Skip(1));
            this.CurrentZone = first;
            this.CurrentZoneEndsAt = DateTime.UtcNow + first.Duration;
            this.Reason = reason;
            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);

            this.zoneController.Open(first.ZoneId);
            this.timer.Change(first.Duration, TimeSpan.Zero);
        }
    }

    public void Skip(ZoneCloseReason reason)
    {
        lock (this.nextZones)
        {
            if (this.CurrentZone == null)
            {
                return;
            }

            if (this.nextZones.Count == 0)
            {
                this.Stop(reason);
                return;
            }

            ZoneDuration aborted = this.GetAbortedZone();
            this.log.Write(new ZoneClosed(DateTime.UtcNow, aborted.ZoneId, aborted.Duration, reason));

            ZoneDuration next = this.nextZones[0];
            this.log.Write(new ZoneOpened(DateTime.UtcNow, next.ZoneId, next.Duration, this.Reason));

            this.nextZones.RemoveAt(0);
            this.CurrentZone = next;
            this.CurrentZoneEndsAt = DateTime.UtcNow + next.Duration;
            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);

            this.zoneController.Open(next.ZoneId);
            this.timer.Change(next.Duration, TimeSpan.Zero);

        }
    }

    public void Stop(ZoneCloseReason reason)
    {
        lock (this.nextZones)
        {
            if (this.CurrentZone is null || this.CurrentZoneEndsAt is null)
            {
                return;
            }

            ZoneDuration aborted = this.GetAbortedZone();
            this.log.Write(new ZoneClosed(DateTime.UtcNow, aborted.ZoneId, aborted.Duration, reason));

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
            ZoneDuration current = this.CurrentZone!;
            this.log.Write(new ZoneClosed(DateTime.UtcNow, current.ZoneId, current.Duration, ZoneCloseReason.Completed));
            if (this.nextZones.Count == 0)
            {
                this.nextZones.Clear();
                this.CurrentZone = null;
                this.CurrentZoneEndsAt = null;

                this.zoneController.Close();
                this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
            else
            {
                ZoneDuration next = this.nextZones[0];
                this.log.Write(new ZoneOpened(DateTime.UtcNow, next.ZoneId, next.Duration, this.Reason));

                this.nextZones.RemoveAt(0);
                this.CurrentZone = next;
                this.CurrentZoneEndsAt = DateTime.UtcNow + next.Duration;

                this.zoneController.Open(next.ZoneId);
                this.timer.Change(next.Duration, Timeout.InfiniteTimeSpan);
            }

            this.CurrentZoneChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private ZoneDuration GetAbortedZone()
    {
        return new ZoneDuration(this.CurrentZone!.ZoneId, this.CurrentZone.Duration - (this.CurrentZoneEndsAt!.Value - DateTime.UtcNow));
    }

    private static ZoneCloseReason ToStopReason(ZoneOpenReason reason) => reason switch
    {
        ZoneOpenReason.Manual => ZoneCloseReason.Manual,
        ZoneOpenReason.Algorithm or ZoneOpenReason.FallbackAlgorithm => ZoneCloseReason.Algorithm,
        _ => throw new ArgumentException("Invalid start reason")
    };
}
