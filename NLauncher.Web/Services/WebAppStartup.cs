using MudBlazor;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps.Running;

namespace NLauncher.Web.Services;

public class WebAppStartup : IAppStartup
{
    public ValueTask<AppHandle?> StartAsync(Guid appId, AppInstall install, IDialogService dialogService)
    {
        return ValueTask.FromResult<AppHandle?>(null);
    }
}
