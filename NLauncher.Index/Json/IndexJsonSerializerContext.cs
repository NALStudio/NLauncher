using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Json;

[JsonSerializable(typeof(IndexManifest))]
[JsonSerializable(typeof(AppManifest))]
[JsonSerializable(typeof(ImmutableArray<IndexAsset>?))] // for IndexAssetCollectionJsonConverter.cs
internal partial class IndexJsonSerializerContext : JsonSerializerContext
{
}