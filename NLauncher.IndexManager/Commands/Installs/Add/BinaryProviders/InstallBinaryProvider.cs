using System.Collections.Immutable;

namespace NLauncher.IndexManager.Commands.Installs.Add.BinaryProviders;
internal abstract class InstallBinaryProvider
{
    public record class FileData(Uri DownloadUrl, ImmutableArray<byte> Hash, ImmutableArray<string> Files);

    public abstract string DisplayName { get; }

    public abstract Task<FileData?> LoadFileDataAsync();
}
