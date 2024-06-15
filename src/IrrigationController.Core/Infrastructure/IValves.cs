namespace IrrigationController.Core.Infrastructure
{
    public interface IValves
    {
        void Open(int valveId);

        void Close(int valveId);
    }
}
