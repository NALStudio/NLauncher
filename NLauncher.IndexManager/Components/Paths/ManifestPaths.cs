using System.Globalization;

namespace NLauncher.IndexManager.Components.Paths;

internal sealed class ManifestPaths : DirectoryPathProvider
{
    public string AssetsDirectory { get; }

    public ManifestPaths(string directory) : base(directory)
    {
        AssetsDirectory = Path.Join(directory, "assets");

        ManifestFile = Path.Join(directory, "manifest.json");
        DescriptionFile = Path.Join(directory, "description.md");
    }

    public string ManifestFile { get; }
    public string DescriptionFile { get; }

    public IEnumerable<string> EnumerateImageFiles()
    {
        return System.IO.Directory.EnumerateFiles(Directory, "*.png");
    }

    public string VersionFile(uint vernum) => Path.Join(Directory, "version_" + vernum.ToString(CultureInfo.InvariantCulture) + ".json");
    public IEnumerable<string> EnumerateVersions() => System.IO.Directory.EnumerateFiles(Directory, "version_*.json");
}