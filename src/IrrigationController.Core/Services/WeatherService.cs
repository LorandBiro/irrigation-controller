using IrrigationController.Core.Infrastructure;

namespace IrrigationController.Core.Services;

public class WeatherService(IWeatherForecastApi weatherForecastApi) : IWeatherService
{
    private static readonly TimeSpan Past = TimeSpan.FromDays(7);
    private static readonly TimeSpan Forecast = TimeSpan.FromDays(7);

    private DateTime cacheKey;
    private WeatherData[] cache = [];

    public async Task<WeatherData> GetCurrentAsync()
    {
        await this.UpdateCacheAsync();

        return this.cache[this.GetIndex(DateTime.UtcNow)];
    }

    public async Task<WeatherData[]> GetRangeAsync(DateTime start, DateTime end)
    {
        if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The times must be in UTC.", nameof(start));
        }

        if (start > end)
        {
            throw new ArgumentException("The start time must be before the end time.", nameof(start));
        }

        await this.UpdateCacheAsync();

        int startIndex = this.GetIndex(start);
        int endIndex = this.GetIndex(end);
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start, "The start hour is out of range.");
        }

        if (endIndex >= this.cache.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(end), end, "The end hour is out of range.");
        }

        WeatherData[] result = new WeatherData[endIndex - startIndex + 1];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = this.cache[startIndex + i];
        }

        return result;
    }

    private async Task UpdateCacheAsync()
    {
        DateTime currentHour = DateTime.UtcNow.TrimToHour();
        DateTime startHour = currentHour - Past;
        if (this.cacheKey == startHour)
        {
            return;
        }

        this.cache = await weatherForecastApi.GetForecastAsync(startHour, currentHour + Forecast);
        this.cacheKey = startHour;
    }

    private int GetIndex(DateTime t) => (int)(t - this.cacheKey).TotalHours;
}
