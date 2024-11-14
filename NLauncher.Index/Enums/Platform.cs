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
    None = 0,

    Windows = 1 << 0,

    Browser = 1 << 1,

    Android = 1 << 2,
}