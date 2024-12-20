using NLauncher.Index.Json.Converters;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.InstallTracking;

[JsonConverter(typeof(JsonInstallGuidConverter))]
public readonly record struct InstallGuid(Guid AppId, uint VerNum, ushort InstallId)
{
    public static bool TryParse(string input, [MaybeNullWhen(false)] out InstallGuid guid)
    {
        guid = default;

        string[] parts = input.Split(input, '_');
        if (parts.Length != 3)
            return false;

        if (!Guid.TryParse(parts[0], out Guid appId))
            return false;
        if (!uint.TryParse(parts[1], out uint vernum))
            return false;
        if (!ushort.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort insId))
            return false;

        guid = new InstallGuid(appId, vernum, insId);
        return true;
    }

    public static InstallGuid Parse(string input)
    {
        if (TryParse(input, out InstallGuid guid))
            return guid;

        throw new ArgumentException("Could not parse InstallGuid: invalid input.");
    }

    public override string ToString()
    {
        string appId = AppId.ToString();
        string vernum = VerNum.ToString(CultureInfo.InvariantCulture);
        string insId = InstallId.ToString("X4", CultureInfo.InvariantCulture);

        Debug.Assert(!appId.Contains('_'));
        Debug.Assert(!vernum.Contains('_'));
        Debug.Assert(!insId.Contains('_'));

        return $"{appId}_{vernum}_{insId}";
    }
}
