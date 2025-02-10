using Microsoft.Extensions.Logging;
using NLauncher.Code.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NLauncher.Services.Cache;
public class CacheService
{
    private readonly ILogger<CacheService> logger;
    private readonly IStorageService storage;

    public CacheService(ILogger<CacheService> logger, IStorageService storage)
    {
        this.logger = logger;
        this.storage = storage;
    }

    public async ValueTask Write<T>(string key, T value, JsonTypeInfo<T> jsonTypeInfo, TimeSpan validFor)
    {
        CacheHeader header = new()
        {
            Timestamp = CacheHeader.GetCurrentTimestamp(),
            ValidFor = validFor.Ticks / TimeSpan.TicksPerSecond,
        };

        await using MemoryStream ms = new();
        await Serialize(ms, header, value, jsonTypeInfo);
        await storage.WriteCache(key, ms.ToArray());
    }

    // Extracted into a separate function so that the writer will dispose and flush before the MemoryStream is converted into an array
    private static async ValueTask Serialize<T>(Stream stream, CacheHeader header, T value, JsonTypeInfo<T> jsonTypeInfo)
    {
        await using Utf8JsonWriter writer = new(stream);

        writer.WriteStartObject();

        writer.WritePropertyName("header");
        JsonSerializer.Serialize(writer, header, NLauncherJsonContext.Default.CacheHeader);

        writer.WritePropertyName("value");
        JsonSerializer.Serialize(writer, value, jsonTypeInfo);

        writer.WriteEndObject();
    }

    public async ValueTask<T?> TryRead<T>(string key, JsonTypeInfo<T> jsonTypeInfo) where T : class
    {
        byte[] data = await storage.ReadCache(key);
        if (data.Length == 0)
            return null; // Cache file didn't exist

        try
        {
            return Deserialize(data, jsonTypeInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cache '{}' value deserialization failed.", key);
            return null;
        }
    }

    private static T? Deserialize<T>(byte[] data, JsonTypeInfo<T> jsonTypeInfo) where T : class
    {
        Utf8JsonReader reader = new(data);

        ForceRead(ref reader);
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected a top level JSON object.");

        #region Deserialize Header
        ForceRead(ref reader);
        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException($"Unexpected token type: {reader.TokenType}");

        string? p1 = reader.GetString();
        if (p1 != "header")
            throw new JsonException("Cache header must come before value."); // so that the value is only parsed once the header is validated.

        ForceRead(ref reader);
        CacheHeader? header = JsonSerializer.Deserialize(ref reader, NLauncherJsonContext.Default.CacheHeader);
        #endregion

        if (header?.IsValid() != true)
            return null;

        #region Deserialize Value
        ForceRead(ref reader);
        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException($"Unexpected token type: {reader.TokenType}");

        string? p2 = reader.GetString();
        if (p2 != "value")
            throw new JsonException("Header must be followed by value.");

        ForceRead(ref reader);
        T value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo) ?? throw new JsonException("Didn't expect value to be null.");
        #endregion

        ForceRead(ref reader);
        if (reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException($"Unexpected token type: {reader.TokenType}");

        if (reader.Read())
            throw new JsonException("Too much data.");

        return value;
    }

    private static void ForceRead(ref Utf8JsonReader reader)
    {
        if (!reader.Read())
            throw new JsonException("Missing data.");
    }
}
