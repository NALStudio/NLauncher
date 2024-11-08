using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components;
internal static class AsciiNormalizer
{
    private static readonly FrozenDictionary<char, char> asciiNormalizeReplacements = new Dictionary<char, char>()
    {
        { 'å', 'a' }, // Bit stupid since å is more of an o sound in Swedish, but fuck you Swedes I'm Finnish and I'll translate it to 'a' instead.
        { 'ä', 'a' },
        { 'ö', 'o' },
        // More replacements can be added later if needed
    }.ToFrozenDictionary();

    /// <summary>
    /// Normalize to ASCII by omitting all non-ascii characters (except for a few exceptions).
    /// </summary>
    /// <remarks>
    /// The following characters are not omitted from output and are instead converted:
    /// <code>
    /// å => a
    /// ä => a
    /// ö => o
    /// </code>
    /// </remarks>
    public static string AsLowerCaseAscii(string s)
    {
        // I tried to make it so that exclamation marks (!), question marks (?), etc. were omitted
        // but it became too much of a hassle to add all these options so I'll just remove them manually for now
        // I can make a specialized path normalization function in the future instead.

        ReadOnlySpan<char> input = s.AsSpan();
        Span<char> output = stackalloc char[input.Length];

        int i = 0;
        foreach (char inputChar in input)
        {
            char c = char.ToLowerInvariant(inputChar);

            bool charValid = false;
            if (char.IsAscii(c))
            {
                charValid = true;
            }
            else if (asciiNormalizeReplacements.TryGetValue(c, out char replacement))
            {
                c = replacement;
                charValid = true;
            }

            if (charValid)
            {
                output[i] = c;
                i++;
            }
        }

        // I'm not gonna use unsafe ref here since I don't know whether this string will shit itself
        // once the stackallocated span is destroyed with the rest of the function
        return new string(output[..i]);
    }
}
