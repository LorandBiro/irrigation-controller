using IrrigationController.Infrastructure;
using IrrigationController.Model;

namespace IrrigationController.Watering;

public class GroundMoistureEstimator
{
    private readonly IStationRepository stationRepository;
    private readonly IWeatherService weatherService;

    public GroundMoistureEstimator(IStationRepository stationRepository, IWeatherService weatherService)
    {
        this.stationRepository = stationRepository ?? throw new ArgumentNullException(nameof(stationRepository));
        this.weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        this.maxPrecipitations = maxPrecipitations;
    }

    public async Task<double[]> EstimateGroundMoisture(DateTime t)
    {
        IReadOnlyCollection<Station> stations = await this.stationRepository.GetAllAsync();
        double[] moisture = new double[stations.Count];
        foreach (Station station in stations)
        {
            
        }
    }
}
