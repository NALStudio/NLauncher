using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps;
public interface IPlatformInstaller
{
    public abstract bool InstallSupported(AppInstall install);
    public virtual bool UninstallSupported(AppInstall install) => InstallSupported(install);
    public virtual bool BrowseLocalFilesSupported(AppInstall install) => false;

    public abstract Task<InstallResult> InstallAsync(Guid appId, AppInstall install, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken);
    public abstract Task<InstallResult> UninstallAsync(Guid appId, AppInstall existingInstall, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken);

    /// <summary>
    /// Returns <see langword="true"/> if local files were opened, otherwise <see langword="false"/>
    /// </summary>
    public virtual ValueTask<bool> BrowseLocalFilesAsync(Guid appId, AppInstall existingInstall)
    {
        return ValueTask.FromResult(false);
    }
}
