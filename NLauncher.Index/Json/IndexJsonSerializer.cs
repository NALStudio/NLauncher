using NLauncher.Index.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Json;

[SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "IndexJsonSerializerContext provides type info for IIndexSerializable classes.")]
[SuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "IndexJsonSerializerContext provides type info for IIndexSerializable classes.")]
public static class IndexJsonSerializer
{
    private static readonly JsonSerializerOptions defaultOptions = CreateDefaultOptions();
    private static readonly Dictionary<IndexSerializationOptions, JsonSerializerOptions> cachedOptions = new();

    #region Serialize
    public static string Serialize<T>(T value, IndexSerializationOptions options = IndexSerializationOptions.None) where T : IIndexSerializable
    {
        return JsonSerializer.Serialize(value, GetOptions(options));
    }

    public static async Task SerializeAsync<T>(Stream utf8json, T value, IndexSerializationOptions options = IndexSerializationOptions.None) where T : IIndexSerializable
    {
        await JsonSerializer.SerializeAsync(utf8json, value, GetOptions(options));
    }
    #endregion

    #region Deserialize
    public static T? Deserialize<T>(string json) where T : IIndexSerializable
    {
        return JsonSerializer.Deserialize<T>(json, defaultOptions);
    }
    public static T? Deserialize<T>(ReadOnlySpan<char> json) where T : IIndexSerializable
    {
        return JsonSerializer.Deserialize<T>(json, defaultOptions);
    }

    public static async Task<T?> DeserializeAsync<T>(Stream utf8json) where T : IIndexSerializable
    {
        return await JsonSerializer.DeserializeAsync<T>(utf8json, defaultOptions);
    }
    #endregion

    #region HttpClient Get
    public static async Task<T?> GetIndexFromJsonAsync<T>(this HttpClient http, string? requestUri) where T : IIndexSerializable
    {
        return await http.GetFromJsonAsync<T?>(requestUri, defaultOptions);
    }
    public static async Task<T?> GetIndexFromJsonAsync<T>(this HttpClient http, Uri? requestUri) where T : IIndexSerializable
    {
        return await http.GetFromJsonAsync<T?>(requestUri, defaultOptions);
    }
    #endregion

    #region HttpContent Read
    public static async Task<T?> ReadIndexFromJsonAsync<T>(this HttpContent content) where T : IIndexSerializable
    {
        return await content.ReadFromJsonAsync<T>(defaultOptions);
    }
    #endregion

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        JsonSerializerOptions opt = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,

            TypeInfoResolver = IndexJsonSerializerContext.Default
        };
        opt.MakeReadOnly();

        return opt;
    }

    private static JsonSerializerOptions GetOptions(IndexSerializationOptions options)
    {
        // If no options, return default
        if (options == IndexSerializationOptions.None)
            return defaultOptions;

        // Try to get cached options
        if (!cachedOptions.TryGetValue(options, out JsonSerializerOptions? serializerOptions))
        {
            // If cached options don't exist, create new instance
            serializerOptions = new(defaultOptions);

            if (options.HasFlag(IndexSerializationOptions.WriteNulls))
                serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            if (options.HasFlag(IndexSerializationOptions.Minify))
                serializerOptions.WriteIndented = false;

            serializerOptions.MakeReadOnly();
            cachedOptions.Add(options, serializerOptions);
        }

        return serializerOptions;
    }
}
