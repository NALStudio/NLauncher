using System.Text.Json.Serialization;

namespace NLauncher.Index.Enums;

[JsonConverter(typeof(JsonNumberEnumConverter<AgeRating>))]
public enum AgeRating
{
    // Do not include AgeRating.Unrated in enum so that we can omit it during minified serialization
    // Unrated = -1,

    R3 = 3,
    R7 = 7,
    R10 = 10,
    R13 = 13,
    R16 = 16,
    R18 = 18
}

public static class AgeRatingEnum
{
    /// <summary>
    /// Get the number value of the age rating or 'unrated' if null.
    /// </summary>
    public static string GetIdentifier(this AgeRating? rating)
    {
        if (rating.HasValue)
            return GetIdentifier(rating.Value);
        else
            return "unrated";
    }

    public static string GetIdentifier(this AgeRating rating)
    {
        return rating.ToString("d");
    }
}