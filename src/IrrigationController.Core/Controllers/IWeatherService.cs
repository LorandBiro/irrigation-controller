namespace IrrigationController.Core.Controllers;

public interface IWeatherService
{
    double[] GetEToByHour(DateTime start, DateTime end);
}
