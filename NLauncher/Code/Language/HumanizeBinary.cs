namespace NLauncher.Code.Language;
public static class HumanizeBinary
{
    public static string HumanizeBytes(decimal bytes) => Humanize(bytes, "B");
    public static string HumanizeBits(decimal bits) => Humanize(bits, "b");

    private static string Humanize(decimal value, string unit)
    {
        // Pick best unit
        (string prefix, decimal divisor) = PickBestUnit(value);

        decimal val = value / divisor;

        // format value to a maximum of three significant digits
        return $"{val:G3} {prefix}{unit}";
    }

    private static (string Prefix, decimal Divisor) PickBestUnit(decimal value)
    {
        const decimal minFormattableValue = 0.9995m; // 0,9995 => 1,0

        // Use binary scale of 1024 instead of the SI scale of 1000;
        const decimal scale = 1024m;

        const decimal kiloDiv = scale;
        const decimal megaDiv = kiloDiv * scale;
        const decimal gigaDiv = megaDiv * scale;

        // giga
        if ((value / gigaDiv) >= minFormattableValue)
            return ("G", gigaDiv);

        // mega
        if ((value / megaDiv) >= minFormattableValue)
            return ("M", megaDiv);

        // kilo
        if ((value / kiloDiv) >= minFormattableValue)
            return ("k", kiloDiv);

        return (string.Empty, 1m);
    }
}
