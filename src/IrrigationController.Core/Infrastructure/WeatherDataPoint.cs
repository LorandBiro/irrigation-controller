namespace IrrigationController.Core;

public class WeatherDataPoint
{
    public DateTime Time { get; }

    public double Precipitation { get; }

    public double PrecipitationProbability { get; }

    public double ReferenceEvapotransipration { get; }
}
