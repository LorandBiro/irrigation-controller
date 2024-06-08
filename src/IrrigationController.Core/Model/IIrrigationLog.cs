namespace IrrigationController.Model;

public interface IIrrigationLog
{
    Task SaveAsync(IrrigationLogEntry entry);

    Task<IReadOnlyCollection<IrrigationLogEntry>> GetAllAsync();
}
