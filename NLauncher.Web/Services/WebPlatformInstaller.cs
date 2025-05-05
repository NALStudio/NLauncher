using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps.Installing;

namespace NLauncher.Web.Services;

public class WebPlatformInstaller : IPlatformInstaller
{
    public bool InstallSupported(AppInstall install) => false;

    public ValueTask<bool> IsInstallFound(Guid appId, AppInstall install)
    {
        return ValueTask.FromResult(false);
    }

    public InstallTask Install(Guid appId, AppInstall install)
    {
        throw new NotSupportedException();
    }

    public InstallTask Uninstall(Guid appId, AppInstall existingInstall)
    {
        throw new NotSupportedException();
    }

    public bool ShortcutSupported(AppInstall install) => false;
    public ValueTask CreateShortcut(IndexEntry app, AppInstall install)
    {
        throw new NotSupportedException();
    }
}
