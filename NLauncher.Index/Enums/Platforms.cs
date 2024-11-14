using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Enums;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<Platforms>))]
public enum Platforms
{
    [JsonStringEnumMemberName("none")]
    None = 0,

    [JsonStringEnumMemberName("windows")]
    Windows = 1 << 0,

    [JsonStringEnumMemberName("browser")]
    Browser = 1 << 1,

    [JsonStringEnumMemberName("android")]
    Android = 1 << 2,
}

public static class PlatformsEnum
{
    public static IEnumerable<Platforms> GetIndividualValues(this Platforms platforms)
    {
        foreach (Platforms value in Enum.GetValues<Platforms>())
        {
            if (platforms.HasFlag(value))
                yield return value;
        }
    }
}