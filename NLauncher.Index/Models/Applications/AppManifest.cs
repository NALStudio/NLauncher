using NLauncher.Index.Enums;
using System.Collections.Immutable;

namespace NLauncher.Index.Models.Applications;

// Record to enable copying using the with statement
public record class AppManifest
{
    public required string DisplayName { get; init; }
    public required Guid Uuid { get; init; }
    public IndexEnvironment? Environment { get; init; }

    public required string Developer { get; init; }
    public required string Publisher { get; init; }

    public required AppRelease Release { get; init; }

    // Do not include AgeRating.Unrated in enum so that we can omit it during minified serialization
    public AgeRating? AgeRating { get; init; }
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
    public sbyte Priority { get; init; } // defaults to 0

    public required ImmutableArray<AppVersion> Versions { get; init; }

    /// <summary>
    /// Returns the version with the largest <see cref="AppVersion.VerNum"/> or <see langword="null"/> if <see cref="Versions"/> array is empty.
    /// </summary>
    public AppVersion? GetLatestVersion()
    {
        return Versions.MaxBy(static v => v.VerNum);
    }

    /// <summary>
    /// Returns the version with the specified <paramref name="vernum"/> or <see langword="null"/> if no such version is found.
    /// </summary>
    /// <remarks>
    /// If <paramref name="vernum"/> is null, the latest version is returned instead.
    /// </remarks>
    public AppVersion? GetVersion(uint? vernum)
    {
        if (vernum.HasValue)
            return Versions.SingleOrDefault(v => v.VerNum == vernum.Value);
        else
            return GetLatestVersion();
    }
}
