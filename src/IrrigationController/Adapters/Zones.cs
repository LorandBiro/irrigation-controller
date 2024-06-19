using IrrigationController.Core.Infrastructure;
using System.Device.Gpio;

namespace IrrigationController.Adapters;

public sealed class Zones(ILogger<Zones> logger, ZonesConfig config) : IZones, IDisposable
{
    private readonly ILogger<Zones> logger = logger;
    private readonly ZonesConfig config = config;
    private readonly GpioController controller = new();

    public void Initialize()
    {
        foreach (int pin in this.config.Pins)
        {
            this.controller.OpenPin(pin, PinMode.Output);
            this.controller.Write(pin, PinValue.Low);
            this.logger.LogDebug("Pin #{Pin} opened for output", pin);
        }
    }

    public void Open(int zoneId)
    {
        this.Write(zoneId, true);
        this.logger.LogDebug("Zone #{Zone} opened", zoneId);
    }

    public void Close(int zoneId)
    {
        this.Write(zoneId, false);
        this.logger.LogDebug("Zone #{Zone} closed", zoneId);
    }

    public void Dispose()
    {
        this.controller.Dispose();
    }

    private void Write(int zoneId, bool value)
    {
        int pin = this.config.Pins[zoneId];
        PinValue pinValue = value ? PinValue.High : PinValue.Low;
        this.controller.Write(pin, pinValue);
    }
}
