using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;
using IrrigationController.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace IrrigationController.Controllers;

[Route("metrics")]
[ApiController]
public class MetricsController(Config config, SoilMoistureEstimator soilMoistureEstimator, ProgramController programController, IZoneRepository zoneRepository, IWeatherService weatherService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        StringBuilder sb = new();

        WeatherData weather = await weatherService.GetCurrentAsync();
        sb.AppendLine($"weather_temperature {weather.Temperature}");
        sb.AppendLine($"weather_precipitation {weather.Precipitation}");
        sb.AppendLine($"weather_precipitation_probability {weather.PrecipitationProbability}");
        sb.AppendLine($"weather_eto {weather.ETo}");
        for (int i = 0; i < config.Zones.Count; i++)
        {
            ZoneInfo zoneInfo = config.Zones[i];
            Zone? zone = zoneRepository.Get(i);
            double soilMoisture = await soilMoistureEstimator.EstimateAsync(i, DateTime.UtcNow);
            sb.AppendLine($"zone_soil_moisture{{zone=\"{zoneInfo.Name}\"}} {soilMoisture}");
            sb.AppendLine($"zone_open{{zone=\"{zoneInfo.Name}\"}} {(programController.CurrentZone?.ZoneId == i ? "1" : "0")}");
            sb.AppendLine($"zone_short_circuit{{zone=\"{zoneInfo.Name}\"}} {(zone?.IsDefective == true ? "1" : "0")}");
        }

        return this.Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }
}
