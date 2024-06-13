namespace IrrigationController.Core
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
            foreach (Valve valve in valveConfig.Valves)
            {
                this.gpio.OpenOutput(valve.Pin);
                this.gpio.Write(valve.Pin, false);
            }

            this.timer = new(this.TimerCallback);
        }

        public int? OpenValveId { get; private set; }

        public event EventHandler<int?>? OpenValveIdChanged;

        public void Open(int valveId)
        {
            foreach (Valve valve in this.valveConfig.Valves)
            {
                this.gpio.Write(valve.Pin, false);
            }

            this.OpenValveId = valveId;
            this.OpenValveIdChanged?.Invoke(this, valveId);

            this.timer.Change(this.valveConfig.ValveDelay, Timeout.InfiniteTimeSpan);
        }

        public void Close()
        {
            foreach (Valve valve in this.valveConfig.Valves)
            {
                this.gpio.Write(valve.Pin, false);
            }

            this.OpenValveId = null;
            this.OpenValveIdChanged?.Invoke(this, null);

            this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }

        private void TimerCallback(object? state)
        {
            if (this.OpenValveId is null)
            {
                return;
            }

            Valve openValve = this.valveConfig.Valves[this.OpenValveId.Value];
            this.gpio.Write(openValve.Pin, true);
        }
    }
}
