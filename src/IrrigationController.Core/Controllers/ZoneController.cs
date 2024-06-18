using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Controllers
{
    public sealed class ZoneController : IDisposable
    {
        private readonly IZones zones;
        private readonly ZoneControllerConfig config;

        private readonly Timer timer;

        public ZoneController(IZones zones, ZoneControllerConfig config)
        {
            this.zones = zones;
            this.config = config;
            this.timer = new(this.TimerCallback);
        }

        public int? OpenZoneId { get; private set; }

        public event EventHandler? OpenZoneIdChanged;

        public void Open(int zoneId)
        {
            lock (this.timer)
            {
                if (this.OpenZoneId is not null)
                {
                    if (this.OpenZoneId == zoneId)
                    {
                        return;
                    }

                    this.zones.Close(this.OpenZoneId.Value);
                }

                this.OpenZoneId = zoneId;
                this.OpenZoneIdChanged?.Invoke(this, EventArgs.Empty);

                this.timer.Change(this.config.Delay, Timeout.InfiniteTimeSpan);
            }
        }

        public void Close()
        {
            lock (this.timer)
            {
                if (this.OpenZoneId is null)
                {
                    return;
                }

                this.zones.Close(this.OpenZoneId.Value);

                this.OpenZoneId = null;
                this.OpenZoneIdChanged?.Invoke(this, EventArgs.Empty);

                this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }

        private void TimerCallback(object? state)
        {
            lock (this.timer)
            {
                if (this.OpenZoneId is null)
                {
                    return;
                }

                this.zones.Open(this.OpenZoneId.Value);
            }
        }
    }
}
