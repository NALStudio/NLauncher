using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Installs;
using NLauncher.Components.Dialogs.Installs.ChooseInstall;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Library;
using NLauncher.Shared.AppHandlers.Base;
using System.Diagnostics;

namespace NLauncher.Services.Apps;
public class AppInstallService
{
    public class AppInstallConfig
    {
        public IDialogService DialogService { get; }
        public AppInstallConfig(IDialogService dialogService)
        {
            DialogService = dialogService;
        }

        public bool VerifyIfNotLatestVersion { get; init; } = true;
        public bool AlwaysAskInstallMethod { get; init; } = false;
    }

    private readonly AppHandlerService appHandlers;
    private readonly LibraryService library;

    public AppInstallService(AppHandlerService appHandlers, LibraryService library)
    {
        this.appHandlers = appHandlers;
        this.library = library;
    }

    /// <summary>
    /// Returns <see langword="true"/> if application installation was started succesfully, otherwise <see langword="false"/>.
    /// </summary>
    public async Task<bool> InstallAsync(AppManifest app, AppInstallConfig config)
    {
        InstallResult result = await InternalInstall(app, config);
        if (result.ErrorMessage is not null)
            await config.DialogService.ShowMessageBox("Error!", result.ErrorMessage);
        return result.IsSuccess;
    }

    public async Task<bool> CanInstall(AppManifest app)
    {
        InstallResult<AppVersion> version = await ResolveVersion(app, null, verifyIfNotLatestVersion: false);
        if (!version.IsSuccess)
            return false;

        return InstallOption.EnumerateFromVersion(version.Value, appHandlers.AllHandlersExceptLink).Any();
    }

    private async Task<InstallResult> InternalInstall(AppManifest app, AppInstallConfig config)
    {
        InstallResult<AppVersion> version = await ResolveVersion(app, config.DialogService, verifyIfNotLatestVersion: config.VerifyIfNotLatestVersion);
        if (!version.IsSuccess)
            return InstallResult.Failed(version);

        InstallResult<InstallOption> resolvedInstall = await ResolveInstall(version.Value, config.DialogService, alwaysAskInstallMethod: config.AlwaysAskInstallMethod);
        if (!resolvedInstall.IsSuccess)
            return InstallResult.Failed(resolvedInstall);

        (AppHandler resolvedHandler, AppInstall install) = resolvedInstall.Value;
        if (resolvedHandler is not InstallAppHandler handler)
            return InstallResult.Cancelled();

        await config.DialogService.ShowMessageBox(null, "App installing has not yet been implemented.");
        return InstallResult.Cancelled();
    }

    private async ValueTask<InstallResult<InstallOption>> ResolveInstall(AppVersion version, IDialogService dialogService, bool alwaysAskInstallMethod)
    {
        InstallOption[] autoInstalls = InstallOption.EnumerateFromVersion(version, appHandlers.InstallAppHandlers).ToArray();
        if (autoInstalls.Length == 1 && !alwaysAskInstallMethod) // If only one autoinstaller, use it automatically
            return InstallResult.Success(autoInstalls[0]);

        InstallOption[] validInstalls = InstallOption.EnumerateFromVersion(version, appHandlers.AllHandlersExceptLink).ToArray();
        if (validInstalls.Length < 1) // if no installers, error
            return InstallResult.Errored<InstallOption>($"This application version is not supported on {PlatformsEnum.GetCurrentPlatform()}.");

        // Multiple autoinstallers and any amount of manual installers
        // or zero autoinstallers and one or more manual installer.
        // Ask user to choose.
        CancellableResult<InstallOption> chosenOption = await ChooseInstallDialog.ShowAsync(dialogService, validInstalls);
        if (chosenOption.WasCancelled)
            return InstallResult.Cancelled<InstallOption>();

        return InstallResult.Success(chosenOption.Value);
    }

    private async Task<InstallResult<AppVersion>> ResolveVersion(AppManifest app, IDialogService? dialogService, bool verifyIfNotLatestVersion)
    {
        uint? vernum = null;
        if (await library.HasEntryForApp(app.Uuid))
            vernum = (await library.GetEntry(app.Uuid)).Data.VerNum;

        if (vernum is not null && verifyIfNotLatestVersion)
        {
            Debug.Assert(dialogService is not null);
            CancellableResult<uint?> result = await ConfirmInstallOlderVersionDialog.ShowAsync(dialogService);
            if (result.WasCancelled)
                return InstallResult<AppVersion>.Cancelled();
            vernum = result.Value;
        }

        AppVersion? version = app.GetVersion(vernum);
        if (version is null)
            return InstallResult.Errored<AppVersion>("Version not found.");

        return InstallResult.Success(version);
    }
}
