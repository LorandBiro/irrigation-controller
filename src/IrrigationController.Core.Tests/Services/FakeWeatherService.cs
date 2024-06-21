namespace IrrigationController.Core.Services;

public class FakeWeatherService(double etoPerHour) : IWeatherService
{
    public double[] GetEToByHour(DateTime start, DateTime end)
    {
        start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
        end = new DateTime(end.Year, end.Month, end.Day, end.Hour, 0, 0, DateTimeKind.Utc);

        double[] etoByHour = new double[(int)(end - start).TotalHours + 1];
        for (int i = 0; i < etoByHour.Length; i++)
        {
            etoByHour[i] = etoPerHour;
        }

        return etoByHour;
    }
}
