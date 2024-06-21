namespace IrrigationController.Core.Services;

public interface IWeatherService
{
    double[] GetEToByHour(DateTime start, DateTime end);
}
