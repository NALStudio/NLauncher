namespace NLauncher.Windows;
internal static class Constants
{
    public const string AppDataDirname =
#if DEBUG
        "NALStudio/NLauncherDebug";
#else
        "NALStudio/NLauncher";
#endif

    public static string GetAppDataDirectory()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Join(appData, AppDataDirname);
    }

    /// <summary>
    /// Relative to <c>%LOCALAPPDATA%/{AppDataDirname}/*</c>
    /// </summary>
    public const string WebViewUserDataFolder = "WebView";

    /// <summary>
    /// Relative to <c>%LOCALAPPDATA%/{CacheDirectory}/*</c>
    /// </summary>
    public const string CacheDirectory = "Cache";

    public const string UserAgent = "NLauncher/1.0";


    public const string LauncherLogFileName = "Logs/launcher.log";
    public const string CommandLogFileNameTemplate = "Logs/command_{0}.log";

    public const string LatestReleaseCacheKey = "latest_release";
}
