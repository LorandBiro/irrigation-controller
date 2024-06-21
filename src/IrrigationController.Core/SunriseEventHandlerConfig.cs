namespace IrrigationController.Core;

public record SunriseEventHandlerConfig(IReadOnlyList<(bool Enabled, double MaxPrecipitation, double PrecipitationRate)> Zones);
