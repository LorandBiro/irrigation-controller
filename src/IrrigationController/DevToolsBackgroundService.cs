using IrrigationController.Adapters;
using IrrigationController.Core;

namespace IrrigationController
{
    public class DevToolsBackgroundService(ILogger<DevToolsBackgroundService> logger, ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler, FakeRainSensor fakeRainSensor) : BackgroundService
    {
        private readonly ILogger<DevToolsBackgroundService> logger = logger;
        private readonly ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler = shortCircuitDetectedEventHandler;
        private readonly FakeRainSensor fakeRainSensor = fakeRainSensor;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug("Press 'S' to simulate a short circuit");
            this.logger.LogDebug("Press 'R' to toggle rain");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.S)
                    {
                        this.logger.LogDebug("Simulating a short circuit...");
                        this.shortCircuitDetectedEventHandler.Handle();
                    }
                    else if (key.Key == ConsoleKey.R)
                    {
                        this.logger.LogDebug("Toggling rain...");
                        this.fakeRainSensor.Toggle();
                    }
                }
            }
        }
    }
}
