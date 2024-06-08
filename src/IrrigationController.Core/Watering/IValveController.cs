namespace IrrigationController.Watering;

public interface IValveController
{
    void Open(int stationId);

    void Close();
}
