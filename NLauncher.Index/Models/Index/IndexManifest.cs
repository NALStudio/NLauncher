using NLauncher.Index.Enums;
using NLauncher.Index.Models.News;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Index;
public class IndexManifest
{
    public required IndexEnvironment Environment { get; init; }

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

    public IndexEntry? GetEntryOrNull(Guid id)
    {
        if (TryGetEntry(id, out IndexEntry? entry))
            return entry;
        else
            return null;
    }
}
