using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<AssetType>))]
public enum AssetType
{
    [JsonStringEnumMemberName("icon")]
    Icon,

    [JsonStringEnumMemberName("banner")]
    Banner,

    [JsonStringEnumMemberName("panel")]
    Panel
}

public static class AssetTypeEnum
{
    public readonly record struct Aspect(int Horizontal, int Vertical)
    {
        public static explicit operator float(Aspect aspect) => aspect.Horizontal / (float)aspect.Vertical;
        public static explicit operator double(Aspect aspect) => aspect.Horizontal / (double)aspect.Vertical;
        public static explicit operator decimal(Aspect aspect) => aspect.Horizontal / (decimal)aspect.Vertical;
        public override string ToString() => $"{Horizontal}:{Vertical}";
    }

    public static Aspect AspectRatio(this AssetType type)
    {
        return type switch
        {
            AssetType.Icon => new(1, 1),
            AssetType.Banner => new(16, 9),
            AssetType.Panel => new(2, 3),
            _ => throw new ArgumentException($"Invalid asset type: '{type}'", nameof(type))
        };
    }

    public static AssetType ParseFilename(string filepath)
    {
        bool success = TryParseFilename(filepath, out AssetType t, throwOnError: true);
        Debug.Assert(success); // TryParseFilename should've thrown an error already
        return t;
    }

    public static bool TryParseFilename(string filepath, out AssetType type)
    {
        return TryParseFilename(filepath, out type, throwOnError: false);
    }

    private static bool TryParseFilename(string filepath, [MaybeNullWhen(false)] out AssetType type, bool throwOnError)
    {
        static bool NameMatches(AssetType type, string typename)
        {
            string? name = Enum.GetName(type);
            return string.Equals(name, typename, StringComparison.OrdinalIgnoreCase);
        }

        string filename = Path.GetFileNameWithoutExtension(filepath);
        string typename = filename.Split('_', 1)[0];

        AssetType[] matched = Enum.GetValues<AssetType>().Where(at => NameMatches(at, typename)).ToArray();
        if (matched.Length > 1)
        {
            if (throwOnError)
                throw new ArgumentException($"Multiple types found for filename: {Path.GetFileName(filepath)}");

            type = default;
            return false;
        }

        if (matched.Length < 1)
        {
            if (throwOnError)
                throw new ArgumentException($"No types found for filename: {Path.GetFileName(filepath)}");

            type = default;
            return false;
        }

        type = matched[0];
        return true;
    }
}