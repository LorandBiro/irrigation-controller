using IrrigationController.Core.Domain;

namespace IrrigationController.Core
{
    public class GetFullLogUseCase(IIrrigationLog log)
    {
        public IReadOnlyList<IIrrigationEvent> Execute()
        {
            return log.GetAll();
        }
    }
}
