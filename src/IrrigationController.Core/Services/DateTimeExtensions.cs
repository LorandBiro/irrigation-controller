namespace IrrigationController.Core.Services;

public static class DateTimeExtensions
{
    public static DateTime TrimToHour(this DateTime t) => new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0, t.Kind);
}
