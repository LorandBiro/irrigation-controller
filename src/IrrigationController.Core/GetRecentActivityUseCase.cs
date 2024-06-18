using IrrigationController.Core.Domain;

namespace IrrigationController.Core;

public class GetRecentActivityUseCase(IIrrigationLog log)
{
    public event EventHandler LogUpdated
    {
        add { log.LogUpdated += value; }
        remove { log.LogUpdated -= value; }
    }

    public IReadOnlyList<IIrrigationEvent> Execute()
    {
        return log.Get(10);
    }
}
