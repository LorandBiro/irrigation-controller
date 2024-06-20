namespace IrrigationController.Core;

public record SunriseEventHandlerConfig(IReadOnlyList<(bool Enabled, double PrecipitationPerRun, double PrecipitationRate, double CropCoefficient)> Zones);
