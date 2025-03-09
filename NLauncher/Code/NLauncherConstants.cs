using NLauncher.Index.Enums;

namespace NLauncher.Code;
internal static class NLauncherConstants
{
    public const string LatestReleaseUrl = "https://github.com/NALStudio/NLauncher/releases/latest";

    public static readonly string IndexManifestUrl = $"https://api.github.com/repos/NALStudio/NLauncher-Index/contents/{IndexEnvironmentEnum.GetCurrentEnvironment().GetFilename()}";

    public static class FileNames
    {
        public const string Settings = "settings.json";
        public const string Library = "library.json";
    }

    public static class CacheNames
    {
        public const string Index = "index";

        // Note that other projects might reserve more names than what are listed here.
        // Currently known reserved names other than what are listed above (might not be exhaustive):
        // WINDOWS:
        // - latest_release
    }
}
