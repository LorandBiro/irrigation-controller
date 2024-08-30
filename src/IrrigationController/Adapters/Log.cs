using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters;

public class Log<T>(ILogger<T> logger) : ILog<T>
{
    public void Debug(string message) => logger.LogDebug(message);

    public void Info(string message) => logger.LogInformation(message);

    public void Trace(string message) => logger.LogTrace(message);
}
