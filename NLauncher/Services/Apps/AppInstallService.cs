using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Installs;
using NLauncher.Components.Dialogs.Installs.Choose;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Library;

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

    private readonly LibraryService library;

    public AppInstallService(LibraryService library)
    {
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

    /// <summary>
    /// Returns <see langword="true"/> if the application can be installed.
    /// </summary>
    /// <remarks>
    /// Application cannot be installed if:
    ///     - no installable version can be found
    ///     - or the app has already been installed.
    /// </remarks>
    public async Task<bool> CanInstall(AppManifest app)
    {
        InstallResult<AppVersion> version = await ResolveVersion(app, null, verifyIfNotLatestVersion: false);
        if (!version.IsSuccess)
            return false;

        return version.Value.Installs.Any();
    }

    private async Task<InstallResult> InternalInstall(AppManifest app, AppInstallConfig config)
    {
        InstallResult<AppVersion> version = await ResolveVersion(app, config.DialogService, verifyIfNotLatestVersion: config.VerifyIfNotLatestVersion);
        if (!version.IsSuccess)
            return InstallResult.Failed(version);

        InstallResult<AppInstall> resolvedInstall = await ResolveInstall(version.Value, config.DialogService, alwaysAskInstallMethod: config.AlwaysAskInstallMethod);
        if (!resolvedInstall.IsSuccess)
            return InstallResult.Failed(resolvedInstall);

        AppInstall install = resolvedInstall.Value;
        if (install is not BinaryAppInstall)
            return InstallResult.Cancelled();

        await config.DialogService.ShowMessageBox(null, "App installing has not yet been implemented.");
        return InstallResult.Cancelled();
    }

    private async ValueTask<InstallResult<AppInstall>> ResolveInstall(AppVersion version, IDialogService dialogService, bool alwaysAskInstallMethod)
    {
        Platforms currentPlatform = PlatformsEnum.GetCurrentPlatform();

        AppInstall[] autoInstalls = version.Installs.Where(ins => ins.SupportsAutomaticInstall(currentPlatform)).ToArray();
        if (autoInstalls.Length == 1 && !alwaysAskInstallMethod)
            return InstallResult.Success(autoInstalls[0]);

        // Multiple autoinstallers and any amount of manual installers
        // or zero autoinstallers and one or more manual installer.
        // Ask user to choose.
        CancellableResult<AppInstall> chosenOption = await ChooseInstallDialog.ShowInstallAsync(dialogService, version.Installs);
        if (chosenOption.WasCancelled)
            return InstallResult.Cancelled<AppInstall>();

        return InstallResult.Success(chosenOption.Value);
    }

    private async Task<InstallResult<AppVersion>> ResolveVersion(AppManifest app, IDialogService? dialogService, bool verifyIfNotLatestVersion)
    {
        LibraryEntry? libraryEntry = await library.TryGetEntry(app.Uuid);
        if (libraryEntry?.Data.IsInstalled == true)
            return InstallResult.Errored<AppVersion>("App has already been installed.");

        uint? vernum = libraryEntry?.Data.VerNum;

        // Ask user to confirm that they want to install an older version
        if (vernum.HasValue && verifyIfNotLatestVersion)
        {
            ArgumentNullException.ThrowIfNull(dialogService);
            CancellableResult<uint?> result = await ConfirmInstallOlderVersionDialog.ShowAsync(dialogService, vernum.Value);
            if (result.WasCancelled)
                return InstallResult<AppVersion>.Cancelled();

            // Install the version the user chose (uint for old version, null for latest)
            vernum = result.Value;
        }

        AppVersion? version = app.GetVersion(vernum);
        if (version is null)
            return InstallResult.Errored<AppVersion>("Version not found.");

        return InstallResult.Success(version);
    }
}
