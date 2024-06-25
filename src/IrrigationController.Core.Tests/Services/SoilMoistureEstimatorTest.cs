using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Services;

public class SoilMoistureEstimatorTest
{
    [Fact]
    public async Task NoEvents()
    {
        InMemoryIrrigationLog log = new();
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, Date(7, 1, 6, 0));

        Assert.Equal(0.0, actual);
    }

    [Fact]
    public async Task DryZone()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), ZoneCloseReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, Date(7, 7, 6, 0));

        // We irrigated 10 mm on the first day and ETo is 4.8 mm/day, so the zone is completely dry after a week.
        Assert.Equal(0.0, actual);
    }

    [Fact]
    public async Task FullDay()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), ZoneCloseReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, Date(7, 2, 6, 0));

        // We irrigated 10 mm on the previous day and ETo is 4.8 mm/day in July.
        Assert.Equal(5.2, actual, 0.01);
    }

    [Fact]
    public async Task HalfDay()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), ZoneCloseReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, Date(7, 1, 18, 0));

        // We irrigated 10 mm in the morning and ETo is 4.8 mm/day in July.
        Assert.Equal(7.6, actual, 0.01);
    }

    [Fact]
    public async Task MultipleDays()
    {
        InMemoryIrrigationLog log = new(new ZoneClosed(Date(7, 1, 7, 0), 0, Minutes(60), ZoneCloseReason.Completed), new ZoneClosed(Date(7, 2, 6, 15), 0, Minutes(15), ZoneCloseReason.Completed));
        FakeWeatherService weatherService = new(0.2);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, Date(7, 3, 6, 0));

        // We irrigated 10 mm on the first day, 2.5 mm on the second day and ETo is 4.8 mm/day in July.
        Assert.Equal(2.9, actual, 0.01);
    }

    [Fact]
    public async Task HalfZoneAtStart()
    {
        DateTime now = Date(7, 1, 6, 0);

        InMemoryIrrigationLog log = new(new ZoneClosed(now - SoilMoistureEstimator.Range + Minutes(30), 0, Minutes(60), ZoneCloseReason.Completed));
        FakeWeatherService weatherService = new(0.0);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, now);

        Assert.Equal(5.0, actual, 0.01);
    }

    [Fact]
    public async Task ZoneAtEnd()
    {
        DateTime now = Date(7, 1, 6, 0);

        InMemoryIrrigationLog log = new(new ZoneClosed(now, 0, Minutes(60), ZoneCloseReason.Completed));
        FakeWeatherService weatherService = new(0.0);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, now);

        Assert.Equal(10.0, actual, 0.01);
    }

    [Fact]
    public async Task HalfZoneAtEnd()
    {
        DateTime now = Date(7, 1, 6, 0);

        InMemoryIrrigationLog log = new(new ZoneOpened(now - Minutes(30), 0, Minutes(60), ZoneOpenReason.Manual));
        FakeWeatherService weatherService = new(0.0);
        SoilMoistureEstimator estimator = new(log, new([(10, 10, 1)]), weatherService);

        double actual = await estimator.EstimateAsync(0, now);

        Assert.Equal(5.0, actual, 0.01);
    }

    private static TimeSpan Minutes(int minutes) => TimeSpan.FromMinutes(minutes);
    private static DateTime Date(int month, int day, int hour, int minute) => new(2000, month, day, hour, minute, 0, DateTimeKind.Utc);
}
