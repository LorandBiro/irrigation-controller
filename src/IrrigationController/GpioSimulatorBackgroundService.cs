using IrrigationController.Core.UseCases;

namespace IrrigationController
{
    public class GpioSimulatorBackgroundService(ILogger<GpioSimulatorBackgroundService> logger, ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler) : BackgroundService
    {
        private readonly ILogger<GpioSimulatorBackgroundService> logger = logger;
        private readonly ShortCircuitDetectedEventHandler shortCircuitDetectedEventHandler = shortCircuitDetectedEventHandler;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug("Press 'S' to simulate a short circuit");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.S)
                    {
                        this.logger.LogDebug("Simulating a short circuit...");
                        shortCircuitDetectedEventHandler.Handle();
                    }
                }
            }
        }
    }
}
