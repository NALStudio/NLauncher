using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Installs.Choose;
using NLauncher.Index.Models.Applications;

namespace NLauncher.Services.Apps;
public class AppLinkPlayService
{
    private readonly AppHandlerService appHandlers;

    public AppLinkPlayService(AppHandlerService appHandlers)
    {
        this.appHandlers = appHandlers;
    }

    /// <summary>
    /// Returns an href to the primary website if one can be determined from the link handler options.
    /// </summary>
    public InstallOption? GetPrimaryOption(AppManifest app)
    {
        // Take first option or return null if no options available
        // I wrote this manually since LINQ FirstOrDefault returns default instead of null.
        foreach (InstallOption option in GetOptions(app))
            return option;
        return null;
    }

    public async Task Play(AppManifest app, IDialogService dialogService)
    {
        _ = await ChooseInstallDialog.ShowLinkAsync(dialogService, GetOptions(app));
    }

    public bool CanPlay(AppManifest app) => GetOptions(app).Any();

    private IEnumerable<InstallOption> GetOptions(AppManifest app)
    {
        // Link play always uses the latest version
        AppVersion? version = app.GetLatestVersion();
        if (version is null)
            return Enumerable.Empty<InstallOption>();

        return InstallOption.EnumerateFromVersion(version, appHandlers.LinkAppHandlers);
    }
}
