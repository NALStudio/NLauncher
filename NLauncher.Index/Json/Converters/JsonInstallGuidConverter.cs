using NLauncher.Index.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Json.Converters;

// Public so that other JsonContexts can use this outside of this assembly
public class JsonInstallGuidConverter : JsonConverter<InstallGuid>
{
    public override InstallGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string guid = reader.GetString() ?? throw new JsonException();
        return InstallGuid.Parse(guid);
    }

    public override void Write(Utf8JsonWriter writer, InstallGuid value, JsonSerializerOptions options)
    {
        string guid = value.ToString();
        writer.WriteStringValue(guid);
    }
}
