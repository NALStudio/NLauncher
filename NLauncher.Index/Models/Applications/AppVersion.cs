using NLauncher.Index.Models.Applications.Installs;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Applications;
public record class AppVersion
{
    [JsonPropertyName("vernum")]
    public required uint VerNum { get; init; }

    public required string Identifier { get; init; }

    public ushort? MinRecommendedResolution { get; init; }

    // Use immutable array instead of immutable list as this array will not get mutated ever.
    // Immutable list uses an AVL instead of an array to be more efficient during mutation, but thus its immutable performance suffers.
    public required ImmutableArray<AppInstall> Installs { get; init; }
}
