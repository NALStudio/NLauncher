using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models;
public readonly record struct Color24(byte R, byte G, byte B)
{
    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";

    public static Color24 FromHex(string hex)
    {
        ReadOnlySpan<char> hexSpan = hex.AsSpan();
        if (hexSpan[0] == '#')
            hexSpan = hexSpan[1..];

        if (hexSpan.Length != 6)
            throw new ArgumentException("Invalid hex length.");

#pragma warning disable IDE0057 // Use range operator
        bool rValid = TryParseHex(hexSpan.Slice(0, 2), out byte r);
        bool gValid = TryParseHex(hexSpan.Slice(2, 2), out byte g);
        bool bValid = TryParseHex(hexSpan.Slice(4, 2), out byte b);
#pragma warning restore IDE0057 // Use range operator

        if (rValid && gValid && bValid)
            return new Color24(r, g, b);

        throw new ArgumentException("Invalid hex value.");
    }

    private static bool TryParseHex(ReadOnlySpan<char> hex, [MaybeNullWhen(false)] out byte b)
    {
        return byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
    }
}
