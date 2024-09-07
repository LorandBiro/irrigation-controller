using IrrigationController.Core.Services;

namespace IrrigationController.Core;

public class SunriseCalculator(SunriseCalculatorConfig config)
{
    private static readonly DateTime Epoch = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly SunriseCalculatorConfig config = config;

    public DateTime GetStartTime(DateTime date)
    {
        int n = (int)Math.Ceiling((date - Epoch).TotalDays);
        return this.GetSunrise(n);
    }

    private DateTime GetSunrise(int n)
    {
        // https://en.wikipedia.org/wiki/Sunrise_equation
        double meanSolarTime = n - (this.config.Longitude / 360.0);
        double solarMeanAnomaly = (357.5291 + 0.98560028 * meanSolarTime) % 360.0;
        double equationOfTheCenter = 1.9148 * Sin(solarMeanAnomaly) + 0.02 * Sin(2 * solarMeanAnomaly) + 0.0003 * Sin(3 * solarMeanAnomaly);
        double eclipticLongitude = (solarMeanAnomaly + equationOfTheCenter + 180 + 102.9372) % 360.0;
        double solarTransit = 2451545.0 + meanSolarTime + 0.0053 * Sin(solarMeanAnomaly) - 0.0069 * Sin(2 * eclipticLongitude);
        double declinationOfTheSun = Asin(Sin(eclipticLongitude) * Sin(23.4397));
        double hourAngle = Acos((Sin(-0.833) - Sin(this.config.Latitude) * Sin(declinationOfTheSun)) / Cos(this.config.Latitude) * Cos(declinationOfTheSun));
        return FromJulian(solarTransit - hourAngle / 360.0);
    }

    private static double Sin(double degrees) => Math.Sin(Rad(degrees));
    private static double Cos(double degrees) => Math.Cos(Rad(degrees));
    private static double Asin(double d) => Deg(Math.Asin(d));
    private static double Acos(double d) => Deg(Math.Acos(d));
    private static double Rad(double degrees) => degrees * Math.PI / 180.0;
    private static double Deg(double radians) => radians * 180.0 / Math.PI;

    // https://stackoverflow.com/questions/32676664/julian-date-to-datetime-including-hours-and-minute
    public static DateTime FromJulian(double julianDate) => new((long)((julianDate - 1721425.5) * TimeSpan.TicksPerDay), DateTimeKind.Utc);
}
