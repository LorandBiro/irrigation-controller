namespace IrrigationController.Core.Services;

public class FallbackWeatherService : IWeatherService
{
    // Numbers are in mm/day, based on the following sources:
    // https://pazsitdoktor.hu/pazsitontozes
    // https://hu.wikipedia.org/wiki/Magyarorsz%C3%A1g_%C3%A9ghajlata
    private static readonly double[] EToByMonth = [0, 0, 0, 2.5, 3.5, 3.5, 5, 5, 3.5, 2.5, 0, 0];

    public double[] GetEToByHour(DateTime start, DateTime end)
    {
        if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The times must be in UTC.", nameof(start));
        }

        start = start.TrimToHour();
        end = end.TrimToHour();

        double[] etByHour = new double[(int)(end - start).TotalHours + 1];
        for (int i = 0; i < etByHour.Length; i++)
        {
            DateTime t = start.AddHours(i);
            double eto = EToByMonth[t.Month - 1] / 24;
            etByHour[i] = eto;
        }

        return etByHour;
    }
}
