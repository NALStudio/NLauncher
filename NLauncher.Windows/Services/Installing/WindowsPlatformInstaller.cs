using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps.Installing;

namespace NLauncher.Windows.Services.Installing;
public class WindowsPlatformInstaller : IPlatformInstaller
{
    private readonly IServiceProvider serviceProvider;
    public WindowsPlatformInstaller(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public bool InstallSupported(AppInstall install)
    {
        return install is BinaryAppInstall bai && bai.Platform.HasFlag(Platforms.Windows);
    }

    public InstallTask Install(Guid appId, AppInstall install)
    {
        return new WindowsBinaryInstallTask(
            serviceProvider.GetRequiredService<ILogger<WindowsBinaryInstallTask>>(),
            appId,
            (BinaryAppInstall)install,
            uninstall: false
        );
    }

    public InstallTask Uninstall(Guid appId, AppInstall existingInstall)
    {
        return new WindowsBinaryInstallTask(
            serviceProvider.GetRequiredService<ILogger<WindowsBinaryInstallTask>>(),
            appId,
            (BinaryAppInstall)existingInstall,
            uninstall: true
        );
    }
}
