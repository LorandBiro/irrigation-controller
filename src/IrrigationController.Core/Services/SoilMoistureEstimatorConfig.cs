namespace IrrigationController.Core.Services;

public record SoilMoistureEstimatorConfig(IReadOnlyList<(double MaxPrecipitation, double IrrigationRate, double CropCoefficient)> Zones);
