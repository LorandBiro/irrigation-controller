namespace IrrigationController.Watering;

public class StationConfiguration
{
    public StationConfiguration(int id, double targetPrecipitation, double precipitationRate, double cropCoefficient)
    {
        this.Id = id;
        this.TargetPrecipitation = targetPrecipitation;
        this.PrecipitationRate = precipitationRate;
        this.CropCoefficient = cropCoefficient;
    }

    public int Id { get; }

    public double TargetPrecipitation { get; }

    public double PrecipitationRate { get; }

    public double CropCoefficient { get; }
}
