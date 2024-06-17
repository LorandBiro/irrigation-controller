using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core
{
    public class SunriseEventHandler(IIrrigationLog log)
    {
        private readonly IIrrigationLog log = log;

        public void Handle()
        {
            this.log.Write("Sunrise");
        }
    }
}
