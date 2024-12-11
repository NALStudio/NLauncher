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
        // 
#pragma warning disable IL2026, IL3050 // Disable AOT warnings. We can't use JsonContext as otherwise we lose JsonSerializerOptions settings metadata
        ImmutableArray<IndexAsset> assets = JsonSerializer.Deserialize<ImmutableArray<IndexAsset>>(ref reader, options);
#pragma warning restore IL2026, IL3050 

        return new IndexAssetCollection(assets);
    }

    public override void Write(Utf8JsonWriter writer, IndexAssetCollection value, JsonSerializerOptions options)
    {
#pragma warning disable IL2026, IL3050 // Disable AOT warnings. We can't use JsonContext as otherwise we lose JsonSerializerOptions settings metadata
        JsonSerializer.Serialize(writer, value.Assets, options);
#pragma warning restore IL2026, IL3050
    }
}
