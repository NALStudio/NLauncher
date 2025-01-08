using System.Text;

namespace NLauncher.Code.Language;
public static class HumanizeTime
{
    /// <summary>
    /// Converts the provided <see cref="TimeSpan"/> into a human readable representation of hours, minutes and/or seconds.
    /// </summary>
    /// <remarks>
    /// A maximum of two units is displayed in the result.
    /// </remarks>
    public static string HumanizeTimeSpan(TimeSpan timespan)
    {
        // Total (has to be computed manually)
        long hours = timespan.Ticks / TimeSpan.TicksPerHour;

        // Remaining
        int minutes = timespan.Minutes;
        int seconds = timespan.Seconds;


        StringBuilder sb = new();

        if (hours != 0)
        {
            sb.Append(hours);
            sb.Append(" h");
        }

        // We don't display minutes if hours is over 99
        bool hideMinutes = Math.Abs(hours) > 99;
        if (!hideMinutes && minutes != 0)
        {
            TryAddLeadingSpace(ref sb);
            sb.Append(minutes);
            sb.Append(" min");
        }

        // Only display seconds when hours aren't displayed
        if (hours == 0)
        {
            TryAddLeadingSpace(ref sb);
            sb.Append(seconds);
            sb.Append(" s");
        }

        return sb.ToString();
    }

    private static bool TryAddLeadingSpace(ref StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            sb.Append(' ');
            return true;
        }

        return false;
    }
}
