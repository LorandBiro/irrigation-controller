using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Infrastructure
{
    public interface IZoneRepository
    {
        event EventHandler Changed;

        Zone? Get(int id);

        IReadOnlyList<Zone> GetAll();

        void Save(Zone zone);
    }
}
