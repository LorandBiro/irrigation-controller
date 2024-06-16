using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Adapters
{
    public class IrrigationLog(ILogger<IrrigationLog> logger) : IIrrigationLog
    {
        private readonly ILogger<IrrigationLog> logger = logger;

        public void Write(string message)
        {
            this.logger.LogInformation(message);
        }
    }
}
