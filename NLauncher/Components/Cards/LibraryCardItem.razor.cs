using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps;
using NLauncher.Services.Library;
using NLauncher.Shared.AppHandlers.Base;

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
    private LibraryService LibraryService { get; set; } = default!;

    /// <summary>
    /// Use <see langword="null"/> for skeleton.
    /// </summary>
    [Parameter, EditorRequired]
    public IndexEntry? Entry { get; set; }

    [Parameter, EditorRequired]
    public LibraryData? LibraryData { get; set; }

    private bool canInstall = false;

    private bool CanPlay => isInstalled || linkPlayHref is not null;
    private bool isInstalled = false;
    private string? linkPlayHref = null;

    protected override async Task OnParametersSetAsync()
    {
        canInstall = false;
        isInstalled = false;
        linkPlayHref = null;

        if (Entry is null)
            return;
        AppManifest app = Entry.Manifest;

        canInstall = await InstallService.CanInstall(app);

        LibraryEntry? libraryEntry = await LibraryService.TryGetEntry(app.Uuid);
        isInstalled = libraryEntry?.Data.IsInstalled == true;

        if (LinkPlayService.CanPlay(app))
        {
            InstallOption? primaryLink = LinkPlayService.GetPrimaryOption(app);
            if (primaryLink is InstallOption opt)
                linkPlayHref = ((LinkAppHandler)opt.Handler).GetHref(opt.Install);
        }

        StateHasChanged();
    }

    private async Task OnCardPressed()
    {
        if (Entry is null)
            return;

        if (linkPlayHref is not null)
            return;

        if (canInstall)
        {
            await InstallService.InstallAsync(Entry.Manifest, new AppInstallService.AppInstallConfig(DialogService));
        }
        else if (isInstalled)
        {
            await DialogService.ShowMessageBox(null, "Game startup has not yet been implemented.");
        }
    }

    private static string GetBackgroundImageCss(IndexAsset? asset)
    {
        if (asset is null)
            return string.Empty;

        return $"background-image: url(\"{asset.Url}\")";
    }
}
