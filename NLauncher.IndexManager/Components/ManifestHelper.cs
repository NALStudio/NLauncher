using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components;
internal static class ManifestHelper
{
    public static IEnumerable<DirectoryInfo> DiscoverManifests(IndexPaths paths)
    {
        string manifestFilename = new ManifestPaths("").ManifestFile;

        foreach (FileInfo fi in new DirectoryInfo(paths.Directory).EnumerateFiles(manifestFilename, SearchOption.AllDirectories))
        {
            // Throw error instead of ignoring file if parent directory is null as we don't want to fail silently.
            yield return fi.Directory ?? throw new InvalidOperationException("Manifest must have a parent directory.");
        }
    }
}
