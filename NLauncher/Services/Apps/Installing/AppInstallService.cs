using Microsoft.Extensions.Logging;
using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Installs;
using NLauncher.Components.Dialogs.Installs.Choose;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Library;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Services.Apps.Installing;
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

    private readonly OrderedDictionary<Guid, RunningAppInstall> installsUnsafe = new();

    public event Action? OnCountChanged;

    private int _activeCount;
    public int ActiveCount => _activeCount;
    public event Action<int>? OnActiveCountChanged;

    private readonly ILogger<AppInstallService> logger;

    private readonly IPlatformInstaller installer;
    private readonly LibraryService library;
    private readonly AppBarMenus appBarMenus;

    public AppInstallService(ILogger<AppInstallService> logger, IPlatformInstaller installer, LibraryService library, AppBarMenus appBarMenus)
    {
        this.logger = logger;
        this.installer = installer;
        this.library = library;
        this.appBarMenus = appBarMenus;
    }

    /// <summary>
    /// Returns <see langword="true"/> if application installation was started succesfully, otherwise <see langword="false"/>.
    /// The app installation is queued into the <see cref="RunningAppInstall"/> service.
    /// </summary>
    public async Task<bool> StartInstallAsync(AppManifest app, AppInstallConfig config)
    {
        InstallResult result = await InternalInstall(app, config);
        if (result.ErrorMessage is not null)
            await config.DialogService.ShowMessageBox("Error!", result.ErrorMessage);

        bool success = result.IsSuccess;
        if (success)
            appBarMenus.OpenDownloads();
        return success;
    }

    public RunningAppInstall[] GetInstalls()
    {
        lock (installsUnsafe)
            return installsUnsafe.Values.ToArray();
    }

    public bool TryGetInstall(Guid appId, [MaybeNullWhen(false)] out RunningAppInstall install)
    {
        lock (installsUnsafe)
            return installsUnsafe.TryGetValue(appId, out install);
    }

    public bool IsInstalling(Guid appId)
    {
        if (TryGetInstall(appId, out RunningAppInstall? install))
            return !install.IsFinished;
        else
            return false;
    }

    /// <summary>
    /// Returns <see langword="false"/> if install is not found or install is currently running.
    /// </summary>
    public bool TryRemoveInstall(Guid appId)
    {
        lock (installsUnsafe)
        {
            if (!installsUnsafe.TryGetValue(appId, out RunningAppInstall? install))
                return false;

            if (!install.IsFinished)
                return false;

            if (installsUnsafe.Remove(appId, out RunningAppInstall? removed))
                UnsubscribeEvents(removed);
            else
            {
                logger.LogError("Install removal was unsuccesful even though all checks passed.");
                return false;
            }
        }

        OnCountChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the application can be installed.
    /// </summary>
    /// <remarks>
    /// Application cannot be installed if:
    /// <list type="bullet">
    ///     <item>
    ///         <description>no installable version can be found,</description>
    ///     </item>
    ///     <item>
    ///         <description>the app has already been installed</description>
    ///     </item>
    ///     <item>
    ///         <description>or the install can be handled by <see cref="AppLinkPlayService"/> if <paramref name="includeLinkHandled"/> is <see langword="false"/>.</description>
    ///     </item>
    /// </list>
    /// </remarks>
    public async Task<bool> CanInstall(AppManifest app, bool includeLinkHandled = true)
    {
        InstallResult<AppVersion> version = await ResolveVersion(app, null, verifyIfNotLatestVersion: false);
        if (!version.IsSuccess)
            return false;

        ImmutableArray<AppInstall> installs = version.Value.Installs;
        if (includeLinkHandled)
            return installs.Length > 0;
        else
            return installs.Any(static ins => !AppLinkPlayService.CanPlay(ins));
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

        if (!installer.InstallSupported(install))
            return InstallResult.Errored("This install is not supported by the current platform.");

        RunningAppInstall runningInstall = new(library, installer, app, version.Value, install);
        if (!TryAddInstall(app.Uuid, runningInstall))
            return InstallResult.Errored("Application is already installing.");

        runningInstall.Start();
        return InstallResult.Success();
    }

    private bool TryAddInstall(Guid appId, RunningAppInstall newInstall)
    {
        lock (installsUnsafe)
        {
            if (installsUnsafe.TryGetValue(appId, out RunningAppInstall? existingInstall))
            {
                if (!existingInstall.IsFinished)
                    return false;

                UnsubscribeEvents(existingInstall);
            }

            SubscribeEvents(newInstall);
            installsUnsafe[appId] = newInstall;
        }

        OnCountChanged?.Invoke();
        return true;
    }

    private async ValueTask<InstallResult<AppInstall>> ResolveInstall(AppVersion version, IDialogService dialogService, bool alwaysAskInstallMethod)
    {
        Platforms currentPlatform = PlatformsEnum.GetCurrentPlatform();

        AppInstall[] autoInstalls = version.Installs.Where(installer.InstallSupported).ToArray();
        if (autoInstalls.Length == 1 && !alwaysAskInstallMethod)
            return InstallResult.Success(autoInstalls[0]);

        // Multiple autoinstallers and any amount of manual installers
        // or zero autoinstallers and one or more manual installer.
        // Ask user to choose.
        CancellableResult<AppInstall> chosenOption = await ChooseInstallDialog.ShowInstallAsync(dialogService, version.Installs, supportsAutomaticInstall: installer.InstallSupported);
        if (chosenOption.WasCancelled)
            return InstallResult.Cancelled<AppInstall>();

        return InstallResult.Success(chosenOption.Value);
    }

    private async Task<InstallResult<AppVersion>> ResolveVersion(AppManifest app, IDialogService? dialogService, bool verifyIfNotLatestVersion)
    {
        LibraryEntry? libraryEntry = await library.TryGetEntry(app.Uuid);
        if (libraryEntry?.Data.IsInstalled == true)
            return InstallResult.Errored<AppVersion>("App has already been installed.");

        uint? vernum = libraryEntry?.Data.ChosenVerNum;

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

    private void IncrementActiveCount()
    {
        int count = Interlocked.Increment(ref _activeCount);
        OnActiveCountChanged?.Invoke(count);
    }

    private void DecrementActiveCount()
    {
        int count = Interlocked.Decrement(ref _activeCount);
        OnActiveCountChanged?.Invoke(count);
    }

    private void SubscribeEvents(RunningAppInstall install)
    {
        install.OnStarted += IncrementActiveCount;
        install.OnBeforeFinish += DecrementActiveCount;
    }

    private void UnsubscribeEvents(RunningAppInstall install)
    {
        install.OnStarted -= IncrementActiveCount;
        install.OnBeforeFinish -= DecrementActiveCount;
    }
}
