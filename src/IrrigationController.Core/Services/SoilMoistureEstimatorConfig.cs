namespace IrrigationController.Core.Services;

public record SoilMoistureEstimatorConfig(IReadOnlyList<(double PrecipitationPerRun, double PrecipitationRate, double CropCoefficient)> Zones);
