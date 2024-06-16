using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Controllers
{
    public sealed class ValveController : IDisposable
    {
        private readonly IValves valves;
        private readonly ValveControllerConfig config;

        private readonly Timer timer;

        public ValveController(IValves valvaes, ValveControllerConfig config)
        {
            this.valves = valvaes;
            this.config = config;
            this.timer = new(this.TimerCallback);
        }

        public int? OpenValveId { get; private set; }

        public event EventHandler? OpenValveIdChanged;

        public void Open(int valveId)
        {
            lock (this.timer)
            {
                if (this.OpenValveId is not null)
                {
                    if (this.OpenValveId == valveId)
                    {
                        return;
                    }

                    this.valves.Close(this.OpenValveId.Value);
                }

                this.OpenValveId = valveId;
                this.OpenValveIdChanged?.Invoke(this, EventArgs.Empty);

                this.timer.Change(this.config.Delay, Timeout.InfiniteTimeSpan);
            }
        }

        public void Close()
        {
            lock (this.timer)
            {
                if (this.OpenValveId is null)
                {
                    return;
                }

                this.valves.Close(this.OpenValveId.Value);

                this.OpenValveId = null;
                this.OpenValveIdChanged?.Invoke(this, EventArgs.Empty);

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
                if (this.OpenValveId is null)
                {
                    return;
                }

                this.valves.Open(this.OpenValveId.Value);
            }
        }
    }
}
