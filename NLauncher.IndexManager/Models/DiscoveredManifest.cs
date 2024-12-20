using NLauncher.Index.Models.Applications;

namespace NLauncher.IndexManager.Models;
public readonly record struct DiscoveredManifest(string FilePath, string DirectoryPath, AppManifest Manifest);
