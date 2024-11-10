using NLauncher.Index.Enums;
using NLauncher.Index.Interfaces;
using NLauncher.Index.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications;

public class AppManifest : IIndexSerializable
{
    public required string DisplayName { get; init; }
    public required Guid Uuid { get; init; }

    public required string Developer { get; init; }
    public required string Publisher { get; init; }

    public required AppRelease Release { get; init; }

    // Do not include AgeRating.Unrated in enum so that we can omit it during minified serialization
    public AgeRating? AgeRating { get; init; }

    [JsonConverter(typeof(NullableColor24HexJsonConverter))]
    public Color24? Color { get; init; }

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
    public sbyte Priority { get; init; } // defaults to 0

    public required ImmutableArray<AppVersion> Versions { get; init; }
}
