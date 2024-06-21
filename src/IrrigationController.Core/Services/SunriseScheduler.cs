namespace IrrigationController.Core.Services;

public sealed class SunriseScheduler : IDisposable
{
    private readonly SunriseCalculator sunriseCalculator;
    private readonly SunriseEventHandler sunriseEventHandler;

    private readonly Timer timer;

    public SunriseScheduler(SunriseCalculator sunriseCalculator, SunriseEventHandler sunriseEventHandler)
    {
        this.sunriseCalculator = sunriseCalculator;
        this.sunriseEventHandler = sunriseEventHandler;

        this.timer = new Timer(this.TimerCallback);
    }

    public event EventHandler? NextSunriseChanged;

    public DateTime NextSunrise { get; private set; }

    public void Initialize()
    {
        this.ScheduleNextSunrise();
    }

    public void Dispose()
    {
        this.timer.Dispose();
    }

    private void ScheduleNextSunrise()
    {
        DateTime now = DateTime.UtcNow;
        DateOnly date = DateOnly.FromDateTime(now);
        DateTime sunrise = this.sunriseCalculator.GetStartTime(date);
        if (sunrise < now)
        {
            sunrise = this.sunriseCalculator.GetStartTime(date.AddDays(1));
        }

        TimeSpan delay = sunrise - now;
        this.timer.Change(delay, TimeSpan.Zero);

        this.NextSunrise = sunrise.ToLocalTime();
        this.NextSunriseChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TimerCallback(object? state)
    {
        this.sunriseEventHandler.Handle();
        this.ScheduleNextSunrise();
    }
}
