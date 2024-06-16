using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Infrastructure
{
    public interface IValveRepository
    {
        event EventHandler Changed;

        Valve? Get(int id);

        IReadOnlyList<Valve> GetAll();

        void Save(Valve valve);
    }
}
