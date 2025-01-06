using MudBlazor;
using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps.Running;
public interface IAppStartup
{
    /// <summary>
    /// Start an instance of the <paramref name="appId"/> application.
    /// </summary>
    /// <remarks>
    /// This method might use the <paramref name="dialogService"/> to display an error message.
    /// </remarks>
    /// <returns>
    /// An app handle to the started app or <see langword="null"/> if startup failed.
    /// </returns>
    public ValueTask<AppHandle?> StartAsync(Guid appId, AppInstall install, IDialogService dialogService);
}
