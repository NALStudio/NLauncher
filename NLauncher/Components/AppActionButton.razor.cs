
using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Index.Models.Applications;
using NLauncher.Services;
using NLauncher.Services.Apps;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.Apps.Running;
using NLauncher.Services.Library;

namespace NLauncher.Components;

// TODO: Update state when download is started
public partial class AppActionButton
{
    [Inject]
    private AppInstallService InstallService { get; set; } = default!;

    [Inject]
    private AppLinkPlayService LinkPlayService { get; set; } = default!;

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

    private bool canInstall;
    private bool isInstalling;
    private bool isInstalled;

    // Use inline styles instead of class names since class padding gets overridden by MudBlazor
    private string GetStyleCss()
    {
        string css = "padding:16px";

        if (Style is not null)
            css = (css + ";" + Style);

        return css;
    }
    private string? GetIcon()
    {
        if (isInstalled || PlayHref is not null)
            return Icons.Material.Rounded.PlayArrow;

        if (canInstall || isInstalling)
            return Icons.Material.Rounded.Download;

        return null;
    }

    private Color GetColor()
    {
        if (isInstalling)
            return Color.Default;
        else
            return Color.Primary;
    }

    protected override void OnInitialized()
    {
        // If App is null, OnParametersSet does not reset these during the first render
        ResetState();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (ReferenceEquals(App, previousApp))
            return;

        ResetState();

        if (App is not null)
        {
            previousApp = App;
            await LoadState(App);
            StateHasChanged();
        }
    }

    private void ResetState()
    {
        canInstall = false;
        isInstalling = false;
        isInstalled = false;
    }

    private async Task LoadState(AppManifest app)
    {
        // These can be initialized in OnParametersSetAsync since LibraryPage updates all of its children every time an install finishes
        canInstall = await InstallService.CanInstall(app);
        isInstalling = InstallService.IsInstalling(app.Uuid);

        LibraryEntry? libraryEntry = await LibraryService.TryGetEntry(app.Uuid);
        isInstalled = libraryEntry?.Data.IsInstalled == true;
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

        Guid appId = App.Uuid;

        if (PlayHref is not null)
        {
            // Link pressed, update timestamp for sorting and exit
            await LibraryService.UpdateEntryAsync(appId);
            return;
        }

        if (InstallService.IsInstalling(appId))
        {
            AppBarMenus.OpenDownloads();
        }
        else if (canInstall)
        {
            _ = await InstallService.StartInstallAsync(App, new AppInstallService.AppInstallConfig(DialogService));
            StateHasChanged(); // Update canInstall
        }
        else if (isInstalled)
        {
            bool success = await RunningService.RunApp(appId, DialogService);
            if (success)
                await LibraryService.UpdateEntryAsync(appId); // update timestamp for sorting
        }
    }
}
