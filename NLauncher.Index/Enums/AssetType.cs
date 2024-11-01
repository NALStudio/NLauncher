using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Enums;

// TODO: Use snake_case
[JsonConverter(typeof(JsonStringEnumConverter<AssetType>))]
public enum AssetType
{
    Icon,
    Banner,
    Panel
}

public static class AssetTypeEnum
{
    public readonly record struct Aspect(int Horizontal, int Vertical)
    {
        public static explicit operator double(Aspect aspect) => aspect.Horizontal / aspect.Vertical;
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
        static bool NameMatches(AssetType type, string typename)
        {
            string? name = Enum.GetName(type);
            return string.Equals(name, typename, StringComparison.OrdinalIgnoreCase);
        }

        string filename = Path.GetFileNameWithoutExtension(filepath);
        string typename = filename.Split('_', 1)[0];

        AssetType[] matched = Enum.GetValues<AssetType>().Where(at => NameMatches(at, typename)).ToArray();
        if (matched.Length > 0)
            throw new ArgumentException($"Multiple types found for filename: {Path.GetFileName(filepath)}");

        return matched[0];
    }
}