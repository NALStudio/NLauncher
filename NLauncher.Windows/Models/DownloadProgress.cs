using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Windows.Models;
public readonly record struct DownloadProgress(long DownloadedBytes, long TotalBytes)
{
    public double Progress => TotalBytes != 0L ? DownloadedBytes / (double)TotalBytes : 0d;

    public static bool TryParse(string input, [MaybeNullWhen(false)] out DownloadProgress p)
    {
        p = default;

        if (!input.EndsWith(')'))
            return false;

        int startingParenthesis = input.IndexOf('(');
        if (startingParenthesis == -1)
            return false;

        const string ofString = " of ";
        int ofIndex = input.IndexOf(ofString, startingParenthesis);
        if (ofIndex == -1)
            return false;

        int firstNumberStart = startingParenthesis + 1; // inclusive
        int firstNumberEnd = ofIndex;

        int secondNumberStart = ofIndex + ofString.Length;
        int secondNumberEnd = input.Length - 1; // exclusive

        ReadOnlySpan<char> downloadedStr = input.AsSpan(firstNumberStart, firstNumberEnd - firstNumberStart);
        ReadOnlySpan<char> totalStr = input.AsSpan(secondNumberStart, secondNumberEnd - secondNumberStart);

        if (!long.TryParse(downloadedStr, out long downloaded))
            return false;
        if (!long.TryParse(totalStr, out long total))
            return false;

        p = new DownloadProgress(downloaded, total);
        return true;
    }

    public override string ToString()
    {
        double percent = DownloadedBytes / (double)TotalBytes;
        return $"{percent:P0} ({DownloadedBytes} of {TotalBytes})";
    }
}
