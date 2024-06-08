namespace IrrigationController.Model;

public interface IStationRepository
{
    Task SaveAsync(Station run);

    Task<IReadOnlyCollection<Station>> GetAllAsync();
}
