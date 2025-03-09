using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Enums;

/// <summary>
/// Limit the application to a specific environment.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<IndexEnvironment>))]
public enum IndexEnvironment : byte
{
    /// <summary>
    /// Publish the app on release environment only.
    /// </summary>
    [JsonStringEnumMemberName("release")]
    Release = 1,

    /// <summary>
    /// Publish the app on development environment only.
    /// </summary>
    [JsonStringEnumMemberName("development")]
    Development = 2
}

public static class IndexEnvironmentEnum
{
    public static string GetFilename(this IndexEnvironment environment, bool brotliCompressed = false)
    {
        string env = environment switch
        {
            IndexEnvironment.Release => "indexmanifest.json",
            IndexEnvironment.Development => "indexmanifest.dev.json",
            _ => throw new ArgumentOutOfRangeException(nameof(environment))
        };

        if (brotliCompressed)
            env += ".br";

        return env;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IndexEnvironment GetCurrentEnvironment()
    {
#if DEBUG
        return IndexEnvironment.Development;
#else
        return IndexEnvironment.Release;
#endif
    }
}