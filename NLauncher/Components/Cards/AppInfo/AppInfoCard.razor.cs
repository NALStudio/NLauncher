using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps;
using NLauncher.Services.Library;
using NLauncher.Shared.AppHandlers.Base;
using NLauncher.Shared.AppHandlers.Shared;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Components.Cards.AppInfo;

public partial class AppInfoCard
{
    [Inject]
    public ILogger<AppInfoCard> Logger { get; set; } = default!;

    [Inject]
    public AppLinkPlayService AppLinkPlayService { get; init; } = default!;

    [Inject]
    public AppInstallService AppInstallService { get; init; } = default!;

    [Inject]
    public AppHandlerService AppHandlerService { get; init; } = default!;

    [Inject]
    public LibraryService LibraryService { get; init; } = default!;

    [Inject]
    public IDialogService DialogService { get; init; } = default!;

    private const int iconSize = 64;
    private static readonly string iconSizePx = $"{iconSize}px";

    [Parameter, EditorRequired]
    public required IndexEntry? Entry { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [MemberNotNullWhen(false, nameof(Entry))]
    private bool IsLoading => Entry is null;

    private bool canAddToLibrary = false;
    private bool isAddingToLibrary = false;

    private bool canLinkPlay = false;
    private InstallOption? linkPlayPrimary = null;
    private bool canInstall = false;

    protected override async Task OnParametersSetAsync()
    {
        canAddToLibrary = false;
        canLinkPlay = false;
        linkPlayPrimary = null;
        canInstall = false;

        if (Entry is null)
            return;

        Guid appId = Entry.Manifest.Uuid;

        bool hasEntry = await LibraryService.HasEntryForApp(appId);
        canAddToLibrary = !hasEntry;

        canLinkPlay = AppLinkPlayService.CanPlay(Entry.Manifest);
        linkPlayPrimary = AppLinkPlayService.GetPrimaryOption(Entry.Manifest);

        canInstall = await AppInstallService.CanInstall(Entry.Manifest);

        StateHasChanged();
    }

    private Platforms GetSupportedPlatforms()
    {
        ImmutableArray<AppInstall>? appInstalls = Entry?.Manifest.GetLatestVersion()?.Installs;

        Platforms supported = Platforms.None;
        foreach (AppInstall install in appInstalls ?? Enumerable.Empty<AppInstall>())
            supported |= install.GetSupportedPlatforms();

        return supported;
    }

    private async Task AddToLibrary()
    {
        if (Entry is not null && canAddToLibrary)
        {
            isAddingToLibrary = true;
            await LibraryService.AddEntryAsync(Entry.Manifest.Uuid);
            canAddToLibrary = false;
            isAddingToLibrary = false;
            StateHasChanged();
        }
    }

    private string? GetLinkPlayHref()
    {
        if (linkPlayPrimary is InstallOption opt)
            return ((LinkAppHandler)opt.Handler).GetHref(opt.Install);
        else
            return null;
    }

    private string GetLinkPlayText()
    {
        const string playNow = "Play Now";

        if (!linkPlayPrimary.HasValue)
            return playNow;

        return linkPlayPrimary.Value.Handler switch
        {
            WebsiteLinkAppHandler => playNow,
            StoreLinkAppHandler => "Open Store Page",
            _ => "Open External Link"
        };
    }

    private async Task LinkPlay(bool alwaysChoose = false)
    {
        if (Entry is null)
            return;
        if (linkPlayPrimary.HasValue && !alwaysChoose)
            return;

        await AppLinkPlayService.Play(Entry.Manifest, DialogService);
    }

    private async Task Install(bool alwaysChoose = false)
    {
        if (Entry is null)
            return;

        AppInstallService.AppInstallConfig settings = new(DialogService)
        {
            AlwaysAskInstallMethod = alwaysChoose
        };

        _ = await AppInstallService.InstallAsync(Entry.Manifest, settings);
    }

    private static string GetReleaseDateString(AppRelease appRelease)
    {
        DateOnly? release = appRelease.ReleaseDate;

        if (release.HasValue)
            return release.Value.ToShortDateString();
        else
            return "TBD";
    }

    private static string? GetPlatformIcon(Platforms platform)
    {
        return platform switch
        {
            Platforms.Windows => Icons.Custom.Brands.MicrosoftWindows,
            Platforms.Browser => Icons.Material.Rounded.Language,
            Platforms.Android => Icons.Material.Rounded.Android,
            _ => null
        };
    }
}
