using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.InstallTracking;
using NLauncher.Index.Models.News;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Index;
public class IndexManifest
{
    public required AppAliases Aliases { get; init; }
    public required IndexMeta Metadata { get; init; }

    public required ImmutableArray<NewsEntry> News { get; init; }

    [JsonPropertyName("index")]
    public required ImmutableArray<IndexEntry> Entries { get; init; }

    private ImmutableDictionary<Guid, IndexEntry>? entriesLookup;

    /// <summary>
    /// Get the entry with the given <paramref name="id"/>.
    /// </summary>
    public bool TryGetEntry(Guid id, [MaybeNullWhen(false)] out IndexEntry entry)
    {
        entriesLookup ??= Entries.ToImmutableDictionary(key => key.Manifest.Uuid);
        return entriesLookup.TryGetValue(id, out entry);
    }

    public bool TryFindInstall(InstallGuid id, [MaybeNullWhen(false)] out AppInstall install)
    {
        install = null;

        if (!TryGetEntry(id.AppId, out IndexEntry? entry))
            return false;

        AppVersion? version = entry.Manifest.GetVersion(id.VerNum);
        if (version is null)
            return false;

        install = version.Installs.SingleOrDefault(ins => ins.Id == id.InstallId);
        return install is not null;
    }
}
