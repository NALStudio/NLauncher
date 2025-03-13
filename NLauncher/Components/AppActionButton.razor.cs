using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Components.Dialogs;
using NLauncher.Index.Models.Applications;
using NLauncher.Services;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.Apps.Running;
using NLauncher.Services.Library;

namespace NLauncher.Components;

// TODO: Update state when download is started
public partial class AppActionButton : IDisposable
{
    [Inject]
    private AppInstallService InstallService { get; set; } = default!;

    [Inject]
    private LibraryService LibraryService { get; set; } = default!;

    [Inject]
    private AppBarMenus AppBarMenus { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private AppRunningService RunningService { get; set; } = default!;

    private AppManifest? previousApp;
    [Parameter, EditorRequired]
    public AppManifest? App { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? PlayHref { get; set; }

    /// <summary>
    /// Propagate clicks and disable the click from being handled by this button.
    /// This can be used to make an entire card handle this button's event using the <see cref="Activate" /> method instead.
    /// </summary>
    [Parameter]
    public bool PropagateClicks { get; set; }

    public event Action? OnStateLoaded;

    private bool isActivating = false;

    public bool IsInstalling => isInstalling;

    private bool canInstall;
    private bool canUpdate;
    private bool isInstalling;
    private bool isInstalled;

    private uint? installedVersion;

    // removed because tracking the application exit was too difficult
    // private bool isPlaying;

    private void ResetState()
    {
        canInstall = false;
        canUpdate = false;
        isInstalling = false;
        isInstalled = false;

        installedVersion = null;
    }

    private async Task LoadState(AppManifest app)
    {
        canInstall = await InstallService.CanInstall(app);
        canUpdate = await InstallService.UpdateAvailable(app);

        LibraryEntry? libraryEntry = await LibraryService.TryGetEntry(app.Uuid);
        isInstalled = libraryEntry?.Data.IsInstalled == true;

        // Application install is not finished when InstallCountChanged calls this function
        // UPDATE: Not anymore, InstallCountChanged is only called after the install has finished.
        if (!isInstalled)
            isInstalling = InstallService.IsInstalling(app.Uuid);

        installedVersion = libraryEntry?.Data?.Install?.VerNum;

        OnStateLoaded?.Invoke();
    }

    private async Task ReloadState()
    {
        ResetState();
        if (App is not null)
            await LoadState(App);

        StateHasChanged();
    }

    private void InstallStartedOrFinished(Guid appId)
    {
        if (appId == App?.Uuid)
            InvokeAsync(ReloadState);
    }

    // Use inline styles instead of class names since class padding gets overridden by MudBlazor
    private string GetStyleCss()
    {
        string css = "padding:16px";

        if (Style is not null)
            css = css + ";" + Style;

        return css;
    }

    public string? GetIcon()
    {
        // if (isPlaying)
        //     return Icons.Material.Rounded.Stop;

        if (PlayHref is not null)
            return Icons.Material.Rounded.OpenInNew;

        if (canUpdate)
            return Icons.Material.Rounded.SystemUpdateAlt;

        if (isInstalled /*|| PlayHref is not null*/) // PlayHref now displays a "Open in new" icon
            return Icons.Material.Rounded.PlayArrow;

        if (canInstall || isInstalling)
            return Icons.Material.Rounded.Download;

        return null;
    }

    // Not used by this component, exposed for other components to use.
    public string? GetStateMessage()
    {
        if (PlayHref is not null)
            return "Open Link";

        if (canUpdate)
            return "Update";

        if (isInstalled)
            return "Play";

        if (isInstalling)
            return "Installing";

        if (canInstall)
            return "Install";

        return null;
    }

    private Color GetColor()
    {
        if (isInstalling)
            return Color.Warning;
        else if (canUpdate)
            return Color.Info;
        else
            return Color.Primary;
    }

    protected override void OnInitialized()
    {
        InstallService.OnInstallStarted += InstallStartedOrFinished;
        InstallService.OnInstallFinished += InstallStartedOrFinished;

        // If App is null, OnParametersSet does not reset these during the first render
        ResetState();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (ReferenceEquals(App, previousApp))
            return;
        previousApp = App;

        await ReloadState();
    }

    private async Task ActivateIfNotDisabled()
    {
        if (PropagateClicks)
            return;

        await Activate();
    }

    /// <summary>
    /// Run the action that is suitable for the current button state.
    /// </summary>
    /// <remarks>
    /// href is not run, it has to be bound dynamically using 
    /// </remarks>
    public async Task Activate()
    {
        if (App is null)
            return;
        if (isActivating)
            return;

        Guid appId = App.Uuid;

        if (PlayHref is not null)
        {
            // Yield first so that the UI has time to react to the Href before updating
            await Task.Yield();

            // Link pressed, update timestamp for sorting and exit
            await LibraryService.UpdateEntryAsync(appId);
            return;
        }

        isActivating = true;
        StateHasChanged();

        bool run = isInstalled;

        if (InstallService.IsInstalling(appId))
        {
            AppBarMenus.OpenDownloads();
            run = false;
        }
        else if (canInstall) // Use variable instead of re-checking since we want to start the app as fast as possible
        {
            _ = await InstallService.StartInstallAsync(App, new AppInstallService.AppInstallConfig(DialogService));
            run = false;
            await ReloadState(); // update install state
        }
        else if (canUpdate)
        {
            bool? update = await AskUpdate();
            if (update == true)
            {
                if (update.Value)
                {
                    _ = await InstallService.StartUpdateAsync(App, new AppInstallService.AppInstallConfig(DialogService));
                    run = false;
                    await ReloadState();
                }
            }

            if (!update.HasValue)
                run = false;

        }

        if (run)
        {
            bool success = await RunningService.RunApp(appId, DialogService);
            if (success)
            {
                await LibraryService.UpdateEntryAsync(appId); // update timestamp for sorting
                await ReloadState(); // update isPlaying
            }
        }

        isActivating = false;
        StateHasChanged();
    }

    private async Task<bool?> AskUpdate()
    {
        AppVersion? current = null;
        AppVersion? available = null;
        if (App is not null)
        {
            if (installedVersion.HasValue)
                current = App.GetVersion(installedVersion.Value);
            available = await InstallService.TryGetAvailableUpdate(App);
        }

        return await UpdateAvailableDialog.ShowAsync(DialogService, current, available);
    }

    public void Dispose()
    {
        InstallService.OnInstallStarted -= InstallStartedOrFinished;
        InstallService.OnInstallFinished -= InstallStartedOrFinished;

        GC.SuppressFinalize(this);
    }
}
