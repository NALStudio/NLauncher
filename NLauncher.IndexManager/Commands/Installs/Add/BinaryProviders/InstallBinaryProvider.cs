using NLauncher.Index.Models.Applications.Installs;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Installs.Add.BinaryProviders;
internal abstract class InstallBinaryProvider
{
    public record class FileData(Uri DownloadUrl, ImmutableArray<byte> Hash);

    public abstract string DisplayName { get; }

    public abstract Task<FileData?> LoadFileDataAsync();
}
