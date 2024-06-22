using IrrigationController.Core.Infrastructure;
using System.Text.Json;

namespace IrrigationController.Adapters;

public class OpenMeteoApi(OpenMeteoApiConfig config) : IWeatherForecastApi
{
    private readonly HttpClient client = new();

    public async Task<WeatherData[]> GetForecastAsync(DateTime start, DateTime end)
    {
        if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The times must be in UTC.", nameof(start));
        }

        if (start > end)
        {
            throw new ArgumentException("The start time must be before the end time.", nameof(start));
        }

        string url = $"https://api.open-meteo.com/v1/forecast?latitude={config.Latitude}&longitude={config.Longitude}&hourly=precipitation_probability,precipitation,et0_fao_evapotranspiration&timezone=UTC&start_hour={start:yyyy-MM-ddTHH:mm}&end_hour={end:yyyy-MM-ddTHH:mm}";
        using HttpResponseMessage response = await this.client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement hourly = document.RootElement.GetProperty("hourly");
        JsonElement precipitationProbability = hourly.GetProperty("precipitation_probability");
        JsonElement precipitation = hourly.GetProperty("precipitation");
        JsonElement eto = hourly.GetProperty("et0_fao_evapotranspiration");

        WeatherData[] forecast = new WeatherData[eto.GetArrayLength()];
        for (int i = 0; i < forecast.Length; i++)
        {
            forecast[i] = new WeatherData(precipitationProbability[i].GetDouble() / 100.0, precipitation[i].GetDouble(), eto[i].GetDouble());
        }

        return forecast;
    }
}
