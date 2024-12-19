using System.Text.Json.Serialization;

namespace NLauncher.Index.Enums;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<Platforms>))]
public enum Platforms
{
    [JsonStringEnumMemberName("none")]
    None = 0,

    [JsonStringEnumMemberName("windows")]
    Windows = 1 << 0,

    [JsonStringEnumMemberName("browser")]
    Browser = 1 << 1,

    [JsonStringEnumMemberName("android")]
    Android = 1 << 2,
}

public static class PlatformsEnum
{
    public static IEnumerable<Platforms> GetIndividualValues(this Platforms platforms)
    {
        foreach (Platforms value in Enum.GetValues<Platforms>())
        {
            if (platforms.HasFlag(value))
                yield return value;
        }
    }

    /// <summary>
    /// Gets the current platform that the application is running in.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The current platform doesn't dictate what games can be played on this specific platform
    /// as we can play <see cref="Platforms.Browser"/> games on <see cref="Platforms.Windows"/> for example.
    /// </para>
    /// <para>The AUTOMATIC installing of an application is limited to the current platform though.</para>
    /// </remarks>
    public static Platforms GetCurrentPlatform()
    {
        if (OperatingSystem.IsWindows())
            return Platforms.Windows;
        if (OperatingSystem.IsBrowser())
            return Platforms.Browser;
        if (OperatingSystem.IsAndroid())
            return Platforms.Android;

        throw new PlatformNotSupportedException();
    }
}