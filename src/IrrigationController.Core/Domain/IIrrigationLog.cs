namespace IrrigationController.Core.Domain;

public interface IIrrigationLog
{
    event EventHandler LogUpdated;

    IReadOnlyList<IIrrigationEvent> Get(int limit);

    IReadOnlyList<IIrrigationEvent> GetAll();

    void Write(IIrrigationEvent e);
}
