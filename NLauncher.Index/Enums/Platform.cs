using System;
using System.Collections.Generic;
using System.Linq;
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