using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

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
