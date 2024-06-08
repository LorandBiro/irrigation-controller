using Innovative.SolarCalculator;

namespace IrrigationController.Core.Domain;

public class StartTimeCalculatorTest
{
    [Fact]
    public void Test1()
    {
        TimeZoneInfo cst = TimeZoneInfo.FindSystemTimeZoneById("CET");
        SolarTimes solarTimes = new SolarTimes(new DateTime(2024,1,1), 47.3537, 19.0971);
        DateTime sunrise = TimeZoneInfo.ConvertTimeFromUtc(solarTimes.Sunrise.ToUniversalTime(), cst);


        // Using the GPS coordinates of Dunaharaszti, Hungary
        StartTimeCalculator startTimeCalculator = new(47.3537, 19.0971, TimeSpan.FromHours(-1.0));

        // Sunrise in Dunaharaszti
        // Tuesday, April 9, 2024 (UTC+2)
        // 06:06
        Assert.Equal(DateTime.Parse("2024-04-09 03:06:51"), startTimeCalculator.GetStartTime(DateOnly.Parse("2024-04-09")), TimeSpan.FromSeconds(1.0));

        // Using sunrise times from https://gml.noaa.gov/grad/solcalc/table.php?lat=47.3537&lon=19.0971&year=2024
        Assert.Equal(DateTime.Parse("2024-01-01 05:31:00"), startTimeCalculator.GetStartTime(DateOnly.Parse("2024-01-01")), TimeSpan.FromSeconds(1.0));
        Assert.Equal(DateTime.Parse("2024-01-02 05:31:00"), startTimeCalculator.GetStartTime(DateOnly.Parse("2024-01-02")), TimeSpan.FromSeconds(1.0));
        Assert.Equal(DateTime.Parse("2024-06-15 01:46:00"), startTimeCalculator.GetStartTime(DateOnly.Parse("2024-06-15")), TimeSpan.FromSeconds(1.0));
        Assert.Equal(DateTime.Parse("2024-06-16 01:46:00"), startTimeCalculator.GetStartTime(DateOnly.Parse("2024-06-16")), TimeSpan.FromSeconds(1.0));
        Assert.Equal(DateTime.Parse("2024-06-17 01:46:00"), startTimeCalculator.GetStartTime(DateOnly.Parse("2024-06-17")), TimeSpan.FromSeconds(1.0));
    }
}
