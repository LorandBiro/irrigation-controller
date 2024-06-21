namespace IrrigationController.Core.Controllers;

public record SoilMoistureEstimatorConfig(IReadOnlyList<(double PrecipitationPerRun, double PrecipitationRate, double CropCoefficient)> Zones);
