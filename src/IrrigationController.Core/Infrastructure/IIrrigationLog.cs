namespace IrrigationController.Core.Infrastructure
{
    public interface IIrrigationLog
    {
        event EventHandler LogUpdated;

        IReadOnlyList<string> Get(int limit);

        IReadOnlyList<string> GetAll();

        void Write(string message);
    }
}
