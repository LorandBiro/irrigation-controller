using IrrigationController.Core.Infrastructure;
using System.Device.Gpio;

namespace IrrigationController.Adapters;

public sealed class RainSensor : IRainSensor, IDisposable
{
    private readonly ILogger<RainSensor> logger;
    private readonly RainSensorConfig config;
    private readonly RainDetectedEventHandler rainDetectedEventHandler;
    private readonly RainClearedEventHandler rainClearedEventHandler;

    private readonly GpioController gpio;
    private readonly Timer rainSensorSamplerTimer;

    public RainSensor(ILogger<RainSensor> logger, RainSensorConfig config, RainDetectedEventHandler rainDetectedEventHandler, RainClearedEventHandler rainClearedEventHandler)
    {
        this.logger = logger;
        this.config = config;
        this.rainDetectedEventHandler = rainDetectedEventHandler;
        this.rainClearedEventHandler = rainClearedEventHandler;
        this.gpio = new();
        this.rainSensorSamplerTimer = new(this.OnRainSensorSamplerCallback);
    }

    public event EventHandler? IsRainingChanged;

    public bool IsRaining { get; private set; }

    public void Initialize()
    {
        this.gpio.OpenPin(this.config.Pin, PinMode.InputPullUp);
        PinValue state = this.gpio.Read(this.config.Pin);
        this.IsRaining = state == PinValue.High;
        this.logger.LogDebug("Rain sensor pin #{Pin} opened for input. Current state: {State}", this.config.Pin, state);

        this.rainSensorSamplerTimer.Change(TimeSpan.Zero, this.config.SamplingInterval);
    }

    public void Dispose()
    {
        this.gpio.Dispose();
    }

    private void OnRainSensorSamplerCallback(object? state)
    {
        bool current = this.gpio.Read(this.config.Pin) == PinValue.High;
        if (current == this.IsRaining)
        {
            return;
        }

        this.IsRaining = current;
        if (this.IsRaining)
        {
            this.logger.LogDebug("Rain detected");
            this.rainDetectedEventHandler.Handle();
        }
        else
        {
            this.logger.LogDebug("Rain cleared");
            this.rainClearedEventHandler.Handle();
        }

        this.IsRainingChanged?.Invoke(this, EventArgs.Empty);
    }
}
