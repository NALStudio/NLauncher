using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Components.Paths;
using NLauncher.IndexManager.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NLauncher.IndexManager.Components;
internal static class ManifestDiscovery
{
    public static IEnumerable<DirectoryInfo> DiscoverManifestDirectories(IndexPaths paths)
    {
        foreach (FileInfo fi in DiscoverManifestFiles(paths))
        {
            DirectoryInfo? parentDir = fi.Directory;
            ThrowIfNoParentDirectory(parentDir);
            yield return parentDir;
        }
    }

    public static IEnumerable<FileInfo> DiscoverManifestFiles(IndexPaths paths)
    {
        string manifestFilename = new ManifestPaths("").ManifestFile;

        foreach (FileInfo fi in new DirectoryInfo(paths.Directory).EnumerateFiles(manifestFilename, SearchOption.AllDirectories))
            yield return fi;
    }

    public static async IAsyncEnumerable<DiscoveredManifest> DiscoverManifestsAsync(IndexPaths paths)
    {
        foreach (FileInfo file in DiscoverManifestFiles(paths))
        {
            AppManifest? manifest;
            await using (FileStream stream = file.OpenRead())
                manifest = await JsonSerializer.DeserializeAsync(stream, IndexJsonContext.Default.AppManifest);

            if (manifest is null)
                throw new InvalidOperationException($"Manifest could not be deserialized: '{file.FullName}'");

            yield return BuildDiscovery(file, manifest);
        }
    }

    private static DiscoveredManifest BuildDiscovery(FileInfo file, AppManifest manifest)
    {
        string filepath = file.FullName;
        string? dirpath = file.DirectoryName;
        ThrowIfNoParentDirectory(dirpath);
        return new DiscoveredManifest(filepath, dirpath, manifest);
    }

    private static void ThrowIfNoParentDirectory<T>([NotNull] T? parentDirectoryOrNull) where T : class
    {
        if (parentDirectoryOrNull is null)
            throw new InvalidOperationException("Manifest must have a parent directory.");
    }
}
