using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps.Installing;
public interface IPlatformInstaller
{
    public abstract bool InstallSupported(AppInstall install);
    public virtual bool UninstallSupported(AppInstall install) => InstallSupported(install);

    public abstract Task<InstallResult> InstallAsync(Guid appId, AppInstall install, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken);
    public abstract Task<InstallResult> UninstallAsync(Guid appId, AppInstall existingInstall, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken);
}
