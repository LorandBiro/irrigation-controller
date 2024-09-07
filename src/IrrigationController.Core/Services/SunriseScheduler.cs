using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Services;

public sealed class SunriseScheduler : IDisposable
{
    private readonly SunriseCalculator sunriseCalculator;
    private readonly SunriseEventHandler sunriseEventHandler;
    private readonly ILog<SunriseScheduler> log;

    private readonly Timer timer;

    public SunriseScheduler(SunriseCalculator sunriseCalculator, SunriseEventHandler sunriseEventHandler, ILog<SunriseScheduler> log)
    {
        this.sunriseCalculator = sunriseCalculator;
        this.sunriseEventHandler = sunriseEventHandler;
        this.log = log;

        this.timer = new Timer(this.TimerCallback);
    }

    public event EventHandler? NextSunriseChanged;

    public DateTime NextSunrise { get; private set; }

    public void Initialize()
    {
        DateTime date = DateTime.UtcNow.Date;
        DateTime sunrise = this.sunriseCalculator.GetStartTime(date);
        if (sunrise < DateTime.UtcNow)
        {
            sunrise = this.sunriseCalculator.GetStartTime(date.AddDays(1));
        }

        this.Schedule(sunrise);
    }

    public void Dispose()
    {
        this.timer.Dispose();
    }

    private void Schedule(DateTime sunrise)
    {
        this.log.Info($"Waiting for next sunrise: {sunrise.ToLocalTime()}");
        this.timer.Change(sunrise - DateTime.UtcNow, TimeSpan.Zero);
        this.NextSunrise = sunrise;
        this.NextSunriseChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TimerCallback(object? state)
    {
        Task.Run(this.sunriseEventHandler.HandleAsync);

        DateTime date = DateTime.UtcNow.Date.AddDays(1);
        DateTime sunrise = this.sunriseCalculator.GetStartTime(date);
        this.Schedule(sunrise);
    }
}
