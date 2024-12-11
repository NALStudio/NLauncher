using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Index.Models.News;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = false, IndentSize = 4)]

// IIndexSerializable
[JsonSerializable(typeof(IndexManifest))] // Index
[JsonSerializable(typeof(IndexMeta))]
[JsonSerializable(typeof(AppManifest))] // Apps
[JsonSerializable(typeof(AppAliases))]
[JsonSerializable(typeof(NewsManifest))] // News
[JsonSerializable(typeof(NewsEntry))]

// for IndexAssetCollectionJsonConverter.cs
[JsonSerializable(typeof(ImmutableArray<IndexAsset>?))]
public partial class IndexJsonContext : JsonSerializerContext
{
    public static IndexJsonContext HumanReadable { get; } = new IndexJsonContext(
        new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = true,
            IndentSize = 4
        }
    );
}