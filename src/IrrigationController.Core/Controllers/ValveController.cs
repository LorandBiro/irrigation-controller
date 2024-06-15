using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Controllers
{
    public sealed class ValveController : IDisposable
    {
        private readonly IGpio gpio;
        private readonly ValveConfig valveConfig;

        private readonly Timer timer;

        public ValveController(IGpio gpio, ValveConfig valveConfig)
        {
            this.gpio = gpio;
            this.valveConfig = valveConfig;
            this.timer = new(this.TimerCallback);
        }

        public int? OpenValveId { get; private set; }

        public event EventHandler<int?>? OpenValveIdChanged;

        public void Init()
        {
            foreach (Valve valve in valveConfig.Valves)
            {
                this.gpio.OpenOutput(valve.Pin);
                this.gpio.Write(valve.Pin, false);
            }
        }

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

                    Valve valve = this.valveConfig.Valves[this.OpenValveId.Value];
                    this.gpio.Write(valve.Pin, false);
                }

                this.OpenValveId = valveId;
                this.OpenValveIdChanged?.Invoke(this, valveId);

                this.timer.Change(this.valveConfig.ValveDelay, Timeout.InfiniteTimeSpan);
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

                Valve valve = this.valveConfig.Valves[this.OpenValveId.Value];
                this.gpio.Write(valve.Pin, false);

                this.OpenValveId = null;
                this.OpenValveIdChanged?.Invoke(this, null);

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

                Valve valve = this.valveConfig.Valves[this.OpenValveId.Value];
                this.gpio.Write(valve.Pin, true);
            }
        }
    }
}
