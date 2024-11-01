using NLauncher.Index.Enums;
using NLauncher.Index.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications;

public class AppManifest
{
    public required string DisplayName { get; init; }
    public required Guid Uuid { get; init; }

    public required string Developer { get; init; }
    public required string Publisher { get; init; }

    public required AppRelease Release { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public AgeRating AgeRating { get; init; } = AgeRating.Unrated;

    [JsonConverter(typeof(NullableColor24HexJsonConverter))]
    public Color24? Color { get; init; } = null;

    /// <summary>
    /// <code>
    ///  0 => Neutral
    /// &gt;0 => Prioritize
    /// &lt;0 => Deprioritize
    /// </code>
    /// </summary>
    /// <remarks>
    /// Values range from -128 to 127 (inclusive)
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public sbyte Priority { get; init; } = 0;

    public required ImmutableArray<AppVersion> Versions { get; init; }
}
