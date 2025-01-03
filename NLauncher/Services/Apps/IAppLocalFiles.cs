using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps;
public interface IAppLocalFiles
{
    /// <summary>
    /// <see langword="null"/> if size cannot be computed.
    /// </summary>
    /// <remarks>
    /// <para>Function might block heavily, use <see cref="Task.Run"/> if this is an issue.</para>
    /// </remarks>
    public abstract long? ComputeSizeInBytes(Guid appId, AppInstall existingInstall);

    public abstract bool OpenFileBrowserSupported(AppInstall existingInstall);

    /// <summary>
    /// Returns <see langword="true"/> if local files were opened, otherwise <see langword="false"/>
    /// </summary>
    public abstract ValueTask<bool> OpenFileBrowserAsync(Guid appId, AppInstall existingInstall);
}
