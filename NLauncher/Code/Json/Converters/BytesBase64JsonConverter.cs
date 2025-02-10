using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLauncher.Code.Json.Converters;
internal class BytesBase64JsonConverter : JsonConverter<byte[]?>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        else
            return reader.GetBytesFromBase64();
    }

    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteBase64StringValue(value);
    }
}
