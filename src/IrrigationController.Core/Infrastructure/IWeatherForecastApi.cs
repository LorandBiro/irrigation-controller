namespace IrrigationController.Core.Infrastructure;

public interface IWeatherForecastApi
{
    Task<WeatherData[]> GetForecastAsync(DateTime start, DateTime end);
}
