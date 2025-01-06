using MudBlazor;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps.Running;

namespace NLauncher.Web.Services;

public class WebAppStartup : IAppStartup
{
    public async ValueTask<IAppHandle?> StartAsync(Guid appId, AppInstall install, IDialogService dialogService)
    {
        _ = await dialogService.ShowMessageBox("Not Supported", "Application startup is not supported on the web platform.");
        return null;
    }
}
