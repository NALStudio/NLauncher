using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;

namespace NLauncher.Services.Apps.Installing;
public interface IPlatformInstaller
{
    abstract bool InstallSupported(AppInstall install);
    virtual bool UninstallSupported(AppInstall install) => InstallSupported(install);

    /// <summary>
    /// A rudimentary check to verify that the app is installed.
    /// </summary>
    /// <remarks>
    /// <para>This is mostly used as a backup when NLauncher is uninstalled and then reinstalled.</para>
    /// <para>This method can be executed regardless of <see cref="InstallSupported"/> or <see cref="UninstallSupported"/>.</para>
    /// </remarks>
    abstract ValueTask<bool> IsInstallFound(Guid appId, AppInstall install);
    abstract InstallTask Install(Guid appId, AppInstall install);
    abstract InstallTask Uninstall(Guid appId, AppInstall existingInstall);

    abstract bool ShortcutSupported(AppInstall install);
    abstract ValueTask CreateShortcut(IndexEntry app, AppInstall install);
}
