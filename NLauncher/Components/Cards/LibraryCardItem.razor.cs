using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using NLauncher.Components.Menus.LibraryCard;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services;
using NLauncher.Services.Apps;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.Apps.Running;
using NLauncher.Services.Library;

namespace NLauncher.Components.Cards;
public partial class LibraryCardItem
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private AppInstallService InstallService { get; set; } = default!;

    [Inject]
    private AppLinkPlayService LinkPlayService { get; set; } = default!;

    [Inject]
    private AppRunningService RunningService { get; set; } = default!;

    [Inject]
    private LibraryService LibraryService { get; set; } = default!;

    [Inject]
    private AppBarMenus AppBarMenus { get; set; } = default!;

    /// <summary>
    /// Use <see langword="null"/> for skeleton.
    /// </summary>
    [Parameter, EditorRequired]
    public IndexEntry? Entry { get; set; }

    [Parameter, EditorRequired]
    public LibraryData? LibraryData { get; set; }

    private LibraryCardMenu? menu;

    private bool canInstall = false;

    private bool CanPlay => isInstalled || linkPlayHref is not null;
    private bool isInstalling = false;
    private bool isInstalled = false;
    private string? linkPlayHref = null;

    private bool mouseInsideMoreButton = false;

    protected override async Task OnParametersSetAsync()
    {
        canInstall = false;
        isInstalling = false;
        isInstalled = false;
        linkPlayHref = null;

        if (Entry is null)
            return;
        AppManifest app = Entry.Manifest;

        // These can be initialized in OnParametersSetAsync since LibraryPage updates all of its children every time an install finishes
        canInstall = await InstallService.CanInstall(app);
        isInstalling = InstallService.IsInstalling(app.Uuid);

        LibraryEntry? libraryEntry = await LibraryService.TryGetEntry(app.Uuid);
        isInstalled = libraryEntry?.Data.IsInstalled == true;

        if (LinkPlayService.CanPlay(app))
            linkPlayHref = LinkPlayService.TryGetPrimaryOption(app)?.GetHref();

        StateHasChanged();
    }

    private async Task OnCardPressed()
    {
        if (Entry is null)
            return;

        Guid appId = Entry.Manifest.Uuid;

        if (linkPlayHref is not null)
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
            _ = await InstallService.StartInstallAsync(Entry.Manifest, new AppInstallService.AppInstallConfig(DialogService));
            StateHasChanged(); // Update canInstall
        }
        else if (isInstalled)
        {
            bool success = await RunningService.RunApp(appId, DialogService);
            if (success)
                await LibraryService.UpdateEntryAsync(appId); // update timestamp for sorting
        }
    }

    private void OpenContextMenu(MouseEventArgs args)
    {
        menu?.Open(args);
    }

    private static string GetBackgroundImageCss(IndexAsset? asset)
    {
        if (asset is null)
            return string.Empty;

        return $"background-image: url(\"{asset.Url}\")";
    }

    private void MouseEnteredMoreButton()
    {
        mouseInsideMoreButton = true;
        StateHasChanged();
    }

    private void MouseLeftMoreButton()
    {
        mouseInsideMoreButton = false;
        StateHasChanged();
    }
}
