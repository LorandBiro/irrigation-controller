using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Services;

public interface IWeatherService
{
    Task<WeatherData> GetCurrentAsync();

    Task<WeatherData[]> GetRangeAsync(DateTime start, DateTime end);
}
