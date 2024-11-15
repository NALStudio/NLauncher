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

// IIndexSerializable
[JsonSerializable(typeof(IndexManifest))] // Index
[JsonSerializable(typeof(IndexMeta))]
[JsonSerializable(typeof(AppManifest))] // Apps
[JsonSerializable(typeof(AppAliases))]
[JsonSerializable(typeof(NewsManifest))] // News
[JsonSerializable(typeof(NewsEntry))]

// for IndexAssetCollectionJsonConverter.cs
[JsonSerializable(typeof(ImmutableArray<IndexAsset>?))]
internal partial class IndexJsonSerializerContext : JsonSerializerContext
{
}