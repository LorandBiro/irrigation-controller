using IrrigationController.Adapters;
using IrrigationController.Core;

namespace IrrigationController;

public class DevToolsBackgroundService(ILogger<DevToolsBackgroundService> logger, ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler, FakeRainSensor fakeRainSensor, SunriseEventHandler sunriseEventHandler) : BackgroundService
{
    private readonly ILogger<DevToolsBackgroundService> logger = logger;
    private readonly ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler = shortCircuitDetectedEventHandler;
    private readonly FakeRainSensor fakeRainSensor = fakeRainSensor;
    private readonly SunriseEventHandler sunriseEventHandler = sunriseEventHandler;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogDebug("Press 'C' to simulate a short-circuit");
        this.logger.LogDebug("Press 'R' to toggle rain");
        this.logger.LogDebug("Press 'S' to simulate sunrise");
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.C)
                {
                    this.logger.LogDebug("Simulating a short-circuit...");
                    this.shortCircuitDetectedEventHandler.Handle();
                }
                else if (key.Key == ConsoleKey.R)
                {
                    this.logger.LogDebug("Toggling rain...");
                    this.fakeRainSensor.Toggle();
                }
                else if (key.Key == ConsoleKey.S)
                {
                    this.logger.LogDebug("Simulating sunrise...");
                    this.sunriseEventHandler.Handle();
                }
            }
        }
    }
}
