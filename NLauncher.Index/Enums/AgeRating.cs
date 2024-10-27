using System.Text.Json.Serialization;

namespace NLauncher.Index.Enums;

[JsonConverter(typeof(JsonNumberEnumConverter<AgeRating>))]
public enum AgeRating
{
    Unrated = -1,

    R3 = 3,
    R7 = 7,
    R10 = 10,
    R13 = 13,
    R16 = 16,
    R18 = 18
}
