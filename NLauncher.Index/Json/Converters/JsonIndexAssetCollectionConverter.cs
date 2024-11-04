using NLauncher.Index.Models.Index;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Json.Converters;

internal class JsonIndexAssetCollectionConverter : JsonConverter<IndexAssetCollection>
{
    public override IndexAssetCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ImmutableArray<IndexAsset>? assets = JsonSerializer.Deserialize(ref reader, IndexJsonSerializerContext.Default.NullableImmutableArrayIndexAsset);
        if (assets.HasValue)
            return new IndexAssetCollection(assets.Value);
        else
            return null;
    }

    public override void Write(Utf8JsonWriter writer, IndexAssetCollection value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Assets, IndexJsonSerializerContext.Default.ImmutableArrayIndexAsset);
    }
}
