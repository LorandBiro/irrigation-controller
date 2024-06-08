using IrrigationController.Core;

namespace IrrigationController.Watering;

public class WateringProgramGenerator
{
    private readonly List<StationConfiguration> stations;
    private readonly IWeatherService weatherService;

    public WateringProgramGenerator(IEnumerable<StationConfiguration> stations, IWeatherService weatherService)
    {
        this.stations = stations?.ToList() ?? throw new ArgumentNullException(nameof(stations));
        this.weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));

        if (this.stations.Count == 0)
        {
            throw new ArgumentException("At least 1 station must be specified.", nameof(stations));
        }
    }

    public WateringProgramStep Generate(DateTime t)
    {
        
    }
}
