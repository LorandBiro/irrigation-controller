using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class GetRecentActivityUseCase(IIrrigationLog log)
    {
        private readonly IIrrigationLog log = log;

        public event EventHandler LogUpdated
        {
            add { log.LogUpdated += value; }
            remove { log.LogUpdated -= value; }
        }

        public IReadOnlyList<string> Execute()
        {
            return log.Get(10);
        }
    }
}
