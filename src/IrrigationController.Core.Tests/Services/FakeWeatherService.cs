using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Services;

public class FakeWeatherService(double etoPerHour) : IWeatherService
{
    public Task<WeatherData> GetCurrentAsync()
    {
        return Task.FromResult(new WeatherData(0, 0, etoPerHour));
    }

    public Task<WeatherData[]> GetRangeAsync(DateTime start, DateTime end)
    {
        start = start.TrimToHour();
        end = end.TrimToHour();

        WeatherData[] weather = new WeatherData[(int)(end - start).TotalHours + 1];
        for (int i = 0; i < weather.Length; i++)
        {
            weather[i] = new WeatherData(0, 0, etoPerHour);
        }

        return Task.FromResult(weather);
    }
}
