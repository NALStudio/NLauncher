using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components;
internal static class ManifestHelper
{
    public static IEnumerable<DirectoryInfo> DiscoverManifestDirectories(IndexPaths paths)
    {
        foreach (FileInfo fi in DiscoverManifestFiles(paths))
            yield return fi.Directory ?? throw new InvalidOperationException("Manifest must have a parent directory.");
    }

    public static IEnumerable<FileInfo> DiscoverManifestFiles(IndexPaths paths)
    {
        string manifestFilename = new ManifestPaths("").ManifestFile;

        foreach (FileInfo fi in new DirectoryInfo(paths.Directory).EnumerateFiles(manifestFilename, SearchOption.AllDirectories))
            yield return fi;
    }

    public static async IAsyncEnumerable<AppManifest> DiscoverManifestsAsync(IndexPaths paths)
    {
        foreach (FileInfo file in DiscoverManifestFiles(paths))
        {
            AppManifest? manifest;
            await using (FileStream stream = file.OpenRead())
            {
                manifest = await IndexJsonSerializer.DeserializeAsync<AppManifest>(stream);
            }

            if (manifest is null)
                throw new InvalidOperationException($"Manifest could not be deserialized: '{file.FullName}'");

            yield return manifest;
        }
    }
}
