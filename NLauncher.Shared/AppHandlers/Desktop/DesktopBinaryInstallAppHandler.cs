using NLauncher.Code.AppInstaller.Pipelines;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Base;

namespace NLauncher.Shared.AppHandlers.Windows;
public class DesktopBinaryInstallAppHandler : InstallAppHandler<BinaryAppInstall>
{
    public override AppInstallPipeline GetInstallPipeline(BinaryAppInstall install)
    {
        return new BinaryInstallPipeline(install.DownloadUrl);
    }
}
