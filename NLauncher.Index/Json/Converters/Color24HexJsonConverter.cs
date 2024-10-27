﻿using NLauncher.Index.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Json.Converters;

internal class Color24HexJsonConverter : JsonConverter<Color24>
{
    public override Color24 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString() ?? throw new JsonException();
        return Color24.FromHex(value);
    }

    public override void Write(Utf8JsonWriter writer, Color24 value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToHex());
    }
}

internal class NullableColor24HexJsonConverter : JsonConverter<Color24?>
{
    public override Color24? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (value is null)
            return null;
        else
            return Color24.FromHex(value);
    }

    public override void Write(Utf8JsonWriter writer, Color24? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToHex());
        else
            writer.WriteNullValue();
    }
}
