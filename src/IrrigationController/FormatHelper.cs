namespace IrrigationController;

public static class FormatHelper
{
    public static string Format(TimeSpan time)
    {
        return time.ToString(time.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss");
    }
}
