using NLauncher.Code.AppInstaller.Pipelines;
using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class InstallAppHandler : AppHandler
{
    public abstract AppInstallPipeline GetInstallPipeline(AppInstall install);
}

public abstract class InstallAppHandler<T> : InstallAppHandler where T : AppInstall
{
    public sealed override bool CanHandle(AppInstall install) => install is T;

    public sealed override AppInstallPipeline GetInstallPipeline(AppInstall install) => GetInstallPipeline((T)install);
    public abstract AppInstallPipeline GetInstallPipeline(T install);
}
