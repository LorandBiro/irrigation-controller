namespace IrrigationController.Core.Services;

public record SoilMoistureEstimatorConfig(IReadOnlyList<(double MaxPrecipitation, double PrecipitationRate, double CropCoefficient)> Zones);
