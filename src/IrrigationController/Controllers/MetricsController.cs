using IrrigationController.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace IrrigationController.Controllers;

[Route("metrics")]
[ApiController]
public class MetricsController(Config config, SoilMoistureEstimator soilMoistureEstimator) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        StringBuilder sb = new StringBuilder();
        foreach ((string name, double soilMoisture) in config.Zones.Select((zone, i) => (zone.Name, soilMoistureEstimator.Estimate(i, DateTime.UtcNow))))
        {
            sb.AppendLine($"soil_moisture{{zone=\"{name}\"}} {soilMoisture}");
        }

        return this.Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }
}
