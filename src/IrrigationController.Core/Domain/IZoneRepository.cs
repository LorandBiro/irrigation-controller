namespace IrrigationController.Core.Domain
{
    public interface IZoneRepository
    {
        event EventHandler Changed;

        Zone? Get(int id);

        IReadOnlyList<Zone> GetAll();

        void Save(Zone zone);
    }
}
