namespace NLauncher.Services.Cache;
public readonly struct CacheHeader
{
    /// <summary>
    /// Seconds since epoch.
    /// </summary>
    public required long Timestamp { get; init; }

    /// <summary>
    /// Seconds
    /// </summary>
    public required long ValidFor { get; init; }

    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    // [JsonConverter(typeof(BytesBase64JsonConverter))]
    // public byte[]? Hash { get; init; }

    public bool IsValid() => GetCurrentTimestamp() < (Timestamp + ValidFor);

    public static long GetCurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
