using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Json.Converters;
internal class JsonHexNumberConverter<T> : JsonConverter<T> where T : IBinaryInteger<T>
{
    private readonly string format;

    public JsonHexNumberConverter() : this(fullWidth: true)
    {
    }

    public JsonHexNumberConverter(bool fullWidth)
    {
        string format;
        if (fullWidth)
        {
            int byteCount = T.AllBitsSet.GetByteCount();
            int hexCount = 2 * byteCount;
            format = "X" + hexCount.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            format = "X";
        }

        this.format = format;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string hex = reader.GetString() ?? throw new JsonException();
        return T.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        string hex = value.ToString(format, CultureInfo.InvariantCulture);
        writer.WriteStringValue(hex);
    }
}
