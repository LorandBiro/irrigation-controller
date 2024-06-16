using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Infrastructure
{
    public interface IValveRepository
    {
        Valve? Get(int id);

        void Save(Valve valve);
    }
}
