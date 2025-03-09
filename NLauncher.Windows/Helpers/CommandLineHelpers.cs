using System.Buffers;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NLauncher.Windows.Helpers;
internal static partial class CommandLineHelpers
{
    // https://stackoverflow.com/questions/5510343/escape-command-line-arguments-in-c-sharp/6040946#6040946
    public static string EscapeStringWindows(string arg)
    {
        // TODO: Optimize this by using spans instead of regex
        // This can be preallocated since output cannot be more than twice the length of the input

        string s = CommandLineRegex().Replace(arg, @"$1$1\" + "\"");
        s = "\"" + TrailingSlashRegex().Replace(s, @"$1$1") + "\"";

        // Verify the behaviour of this method during debug
        Debug.Assert(UnescapeStringWindows(s) == arg);

        return s;
    }

    [GeneratedRegex(@"(\\*)" + "\"")]
    private static partial Regex CommandLineRegex();

    [GeneratedRegex(@"(\\+)$")]
    private static partial Regex TrailingSlashRegex();


    public static string UnescapeStringWindows(string s)
    {
        if (s.Length < 2 || s[0] != '\"' || s[^1] != '\"')
            throw new ArgumentException("Invalid input.", nameof(s));

        ReadOnlySpan<char> input = s.AsSpan(1, s.Length - 2);

        char[]? rented = null;
        // Unescaped string cannot be longer than input
        Span<char> chars = input.Length < 128 ? stackalloc char[input.Length] : (rented = ArrayPool<char>.Shared.Rent(input.Length));
        try
        {
            int written = InternalUnescapeString(ref chars, ref input);
            return new string(chars[..written]);
        }
        finally
        {
            if (rented is not null)
            {
                chars.Clear();
                ArrayPool<char>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    // Reference: https://stackoverflow.com/questions/5510343/escape-command-line-arguments-in-c-sharp/6040946#6040946
    private static int InternalUnescapeString(scoped ref readonly Span<char> span, scoped ref readonly ReadOnlySpan<char> input)
    {
        int spandex = 0; // span index
        int slashCount = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\\')
            {
                slashCount++;
            }
            else if (c == '\"')
            {
                // Slashes ended with ", escape slashes and quotes

                (int slashes, int quotes) = Math.DivRem(slashCount, 2);

                span.Slice(spandex, slashes).Fill('\\');
                spandex += slashes;

                span.Slice(spandex, quotes).Fill('\"');
                spandex += quotes;

                slashCount = 0;
            }
            else
            {
                // Slashes didn't end with ", do not escape slashes

                if (slashCount > 0)
                {
                    span.Slice(spandex, slashCount).Fill('\\');
                    spandex += slashCount;
                    slashCount = 0;
                }

                span[spandex] = c;
                spandex++;
            }
        }

        // Add ending slashes as-is
        if (slashCount > 0)
        {
            span.Slice(spandex, slashCount).Fill('\\');
            spandex += slashCount;
        }

        return spandex;
    }
}
