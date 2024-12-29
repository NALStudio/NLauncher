using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps;

namespace NLauncher.Web.Services;

public class WebPlatformInstaller : IPlatformInstaller
{
    public bool InstallSupported(AppInstall install) => false;

    public Task<InstallResult> InstallAsync(Guid appId, AppInstall install, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<InstallResult> UninstallAsync(Guid appId, AppInstall existingInstall, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
