
namespace IrrigationController.Core.Domain;

public class InMemoryIrrigationLog(params IIrrigationEvent[] events) : IIrrigationLog
{
    private readonly List<IIrrigationEvent> events = events.ToList();

    public event EventHandler? LogUpdated;

    public IReadOnlyList<IIrrigationEvent> Get(int limit)
    {
        return this.events.TakeLast(limit).ToList();
    }

    public IReadOnlyList<IIrrigationEvent> GetAll()
    {
        return this.events;
    }

    public void Write(IIrrigationEvent e)
    {
        this.events.Add(e);
        this.LogUpdated?.Invoke(this, EventArgs.Empty);
    }
}
