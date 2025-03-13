using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps.Installing;
public interface IPlatformInstaller
{
    public abstract bool InstallSupported(AppInstall install);
    public virtual bool UninstallSupported(AppInstall install) => InstallSupported(install);

    public abstract InstallTask Install(Guid appId, AppInstall install);
    public abstract InstallTask Uninstall(Guid appId, AppInstall existingInstall);
}
