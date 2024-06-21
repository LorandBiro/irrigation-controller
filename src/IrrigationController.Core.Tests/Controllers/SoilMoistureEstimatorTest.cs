using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers;

public class SoilMoistureEstimatorTest
{
    [Fact]
    public void NoEvents()
    {
        InMemoryIrrigationLog log = new();
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, Date(7, 1, 6, 0));

        Assert.Equal(0, actual);
    }

    [Fact]
    public void DryZone()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), IrrigationStopReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, Date(7, 7, 6, 0));

        // We irrigated 10 mm on the first day and ETo is 4.8 mm/day, so the zone is completely dry after a week.
        Assert.Equal(0, actual);
    }

    [Fact]
    public void FullDay()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), IrrigationStopReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, Date(7, 2, 6, 0));

        // We irrigated 10 mm on the previous day and ETo is 4.8 mm/day in July.
        Assert.Equal(5.2, actual, 0.01);
    }

    [Fact]
    public void HalfDay()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), IrrigationStopReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, Date(7, 1, 18, 0));

        // We irrigated 10 mm in the morning and ETo is 4.8 mm/day in July.
        Assert.Equal(7.6, actual, 0.01);
    }

    [Fact]
    public void MultipleDays()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), IrrigationStopReason.Completed), new ZoneClosed(Date(7, 2, 6, 15), 0, Minutes(15), IrrigationStopReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, Date(7, 3, 6, 0));

        // We irrigated 10 mm on the first day, 2.5 mm on the second day and ETo is 4.8 mm/day in July.
        Assert.Equal(2.9, actual, 0.01);
    }

    [Fact]
    public void HalfZoneAtStart()
    {
        DateTime now = Date(7, 1, 6, 0);

        InMemoryIrrigationLog log = new(new ZoneClosed(now - SoilMoistureEstimator.Range + Minutes(30), 0, Minutes(60), IrrigationStopReason.Completed));
        FakeWeatherService weatherService = new(0.0);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, now);

        Assert.Equal(5.0, actual, 0.01);
    }

    [Fact]
    public void ZoneAtEnd()
    {
        DateTime now = Date(7, 1, 6, 0);

        InMemoryIrrigationLog log = new(new ZoneClosed(now, 0, Minutes(60), IrrigationStopReason.Completed));
        FakeWeatherService weatherService = new(0.0);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, now);

        Assert.Equal(10.0, actual, 0.01);
    }

    [Fact]
    public void HalfZoneAtEnd()
    {
        DateTime now = Date(7, 1, 6, 0);

        InMemoryIrrigationLog log = new(new ZoneOpened(now - Minutes(30), 0, Minutes(60), IrrigationStartReason.Manual));
        FakeWeatherService weatherService = new(0.0);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = estimator.Estimate(0, now);

        Assert.Equal(5, actual, 0.01);
    }

    private static TimeSpan Minutes(int minutes) => TimeSpan.FromMinutes(minutes);
    private static DateTime Date(int month, int day, int hour, int minute) => new(2000, month, day, hour, minute, 0, DateTimeKind.Utc);
}
