using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class GetFullLogUseCase(IIrrigationLog log)
    {
        public IReadOnlyList<string> Execute()
        {
            return log.GetAll();
        }
    }
}
