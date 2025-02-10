namespace NLauncher.Windows;
internal static class Constants
{
    public const string AppDataDirname = "NALStudio/NLauncher";

    /// <summary>
    /// Relative to <c>%LOCALAPPDATA%/{AppDataDirname}/*</c>
    /// </summary>
    public const string WebViewUserDataFolder = "WebView";

    /// <summary>
    /// Relative to <c>%LOCALAPPDATA%/{CacheDirectory}/*</c>
    /// </summary>
    public const string CacheDirectory = "Cache";

    public const string UserAgent = "NLauncher/1.0";


    public const string LatestReleaseCacheKey = "latest_release";
}
