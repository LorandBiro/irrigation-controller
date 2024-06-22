namespace IrrigationController.Core.Services;

public class FakeWeatherService(double etoPerHour) : IWeatherService
{
    public double[] GetEToByHour(DateTime start, DateTime end)
    {
        start = start.TrimToHour();
        end = end.TrimToHour();

        double[] etoByHour = new double[(int)(end - start).TotalHours + 1];
        for (int i = 0; i < etoByHour.Length; i++)
        {
            etoByHour[i] = etoPerHour;
        }

        return etoByHour;
    }
}
