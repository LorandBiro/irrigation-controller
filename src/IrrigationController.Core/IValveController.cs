namespace IrrigationController.Core
{
    public interface IValveController
    {
        int ValveCount { get; }

        int? OpenValve { get; }

        void Open(int valve);

        void Close();
    }
}
