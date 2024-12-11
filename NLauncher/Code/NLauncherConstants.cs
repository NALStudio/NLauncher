using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Code;
internal static class NLauncherConstants
{
    public const string LatestReleaseUrl = "https://github.com/NALStudio/NLauncher/releases/latest";
    public const string IndexManifestUrl = "https://api.github.com/repos/NALStudio/NLauncher-Index/contents/indexmanifest.json";

    public static class FileNames
    {
        public const string IndexCache = "cache.dat";
        public const string Settings = "settings.json";
        public const string Library = "library.json";
    }
}
