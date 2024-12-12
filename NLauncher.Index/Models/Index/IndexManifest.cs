using NLauncher.Index.Models.News;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexManifest
{
    public required AppAliases Aliases { get; init; }
    public required IndexMeta Metadata { get; init; }

    public required ImmutableArray<NewsEntry> News { get; init; }

    [JsonPropertyName("index")]
    public required ImmutableArray<IndexEntry> Entries { get; init; }

    private ImmutableDictionary<Guid, IndexEntry>? entriesLookup;
    public bool TryGetEntry(Guid id, [MaybeNullWhen(false)] out IndexEntry entry)
    {
        entriesLookup ??= Entries.ToImmutableDictionary(key => key.Manifest.Uuid);
        return entriesLookup.TryGetValue(id, out entry);
    }
}
