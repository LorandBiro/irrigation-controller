namespace IrrigationController.Core.Infrastructure;

public interface IZones
{
    void Open(int zoneId);

    void Close(int zoneId);
}
