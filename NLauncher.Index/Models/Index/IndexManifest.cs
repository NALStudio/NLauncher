﻿using NLauncher.Index.Interfaces;
using NLauncher.Index.Models.News;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexManifest : IIndexSerializable
{
    public required AppAliases Aliases { get; init; }
    public required IndexMeta Metadata { get; init; }

    public required ImmutableArray<NewsEntry> News { get; init; }

    [JsonPropertyName("index")]
    public required ImmutableArray<IndexEntry> Entries { get; init; }
}
