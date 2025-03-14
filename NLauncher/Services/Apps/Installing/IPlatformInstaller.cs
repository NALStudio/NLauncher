﻿using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps.Installing;
public interface IPlatformInstaller
{
    public abstract bool InstallSupported(AppInstall install);
    public virtual bool UninstallSupported(AppInstall install) => InstallSupported(install);

    /// <summary>
    /// A rudimentary check to verify that the app is installed.
    /// </summary>
    /// <remarks>
    /// This is mostly used as a backup when NLauncher is uninstalled and then reinstalled.
    /// </remarks>
    public abstract ValueTask<bool> IsInstallFound(Guid appId, AppInstall install);
    public abstract InstallTask Install(Guid appId, AppInstall install);
    public abstract InstallTask Uninstall(Guid appId, AppInstall existingInstall);
}
