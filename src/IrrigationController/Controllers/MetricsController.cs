using IrrigationController.Core.Domain;
using IrrigationController.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace IrrigationController.Controllers;

[Route("metrics")]
[ApiController]
public class MetricsController(Config config, SoilMoistureEstimator soilMoistureEstimator, ProgramController programController, IZoneRepository zoneRepository) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        StringBuilder sb = new();
        for (int i = 0; i < config.Zones.Count; i++)
        {
            ZoneInfo zoneInfo = config.Zones[i];
            Zone? zone = zoneRepository.Get(i);
            double soilMoisture = soilMoistureEstimator.Estimate(i, DateTime.UtcNow);
            sb.AppendLine($"soil_moisture{{zone=\"{zoneInfo.Name}\"}} {soilMoisture}");
            sb.AppendLine($"zone_open{{zone=\"{zoneInfo.Name}\"}} {(programController.CurrentZone?.ZoneId == i ? "1" : "0")}");
            sb.AppendLine($"zone_short_circuit{{zone=\"{zoneInfo.Name}\"}} {(zone?.IsDefective == true ? "1" : "0")}");
        }

        return this.Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }
}
