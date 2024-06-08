namespace IrrigationController.Core
{
    public interface IValveController
    {
        int ValveCount { get; }

        int? OpenValve { get; }

        void Open(int valveId);

        void Close();
    }
}
