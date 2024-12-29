using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Services;
using NLauncher.Services.Apps;
using NLauncher.Services.Library;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Components.Cards.AppInfo;

public partial class AppInfoCard : IDisposable
{
    [Inject]
    public ILogger<AppInfoCard> Logger { get; set; } = default!;

    [Inject]
    public AppLinkPlayService AppLinkPlayService { get; init; } = default!;

    [Inject]
    public AppInstallService AppInstallService { get; init; } = default!;

    [Inject]
    public LibraryService LibraryService { get; init; } = default!;

    [Inject]
    public AppBarMenus AppBarMenus { get; init; } = default!;

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
    private AppInstall? linkPlayPrimary = null;

    private bool canInstall = false;
    private bool isInstalling = false;
    private bool isInstalled = false;
    private bool isStartingInstall = false;

    protected override void OnInitialized()
    {
        AppInstallService.InstallChanged += OnInstallChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        canAddToLibrary = false;
        canLinkPlay = false;
        linkPlayPrimary = null;

        canInstall = false;
        isInstalling = false;
        isInstalled = false;

        if (Entry is null)
            return;

        Guid appId = Entry.Manifest.Uuid;

        LibraryEntry? libraryEntry = await LibraryService.TryGetEntry(appId);
        canAddToLibrary = !libraryEntry.HasValue;

        canLinkPlay = AppLinkPlayService.CanPlay(Entry.Manifest);
        linkPlayPrimary = AppLinkPlayService.TryGetPrimaryOption(Entry.Manifest);
        await UpdateInstalledState(libraryEntry);

        StateHasChanged();
    }

    // I really don't like this async void function....
    private async void OnInstallChanged()
    {
        if (Entry is null)
            return;

        LibraryEntry? libraryEntry = await LibraryService.TryGetEntry(Entry.Manifest.Uuid);
        await UpdateInstalledState(libraryEntry);
    }

    private async Task UpdateInstalledState(LibraryEntry? libraryEntry)
    {
        if (Entry is null)
            throw new InvalidOperationException("Entry is null");

        bool rerender = false;

        bool canInstall = await AppInstallService.CanInstall(Entry.Manifest, includeLinkHandled: false);
        if (this.canInstall != canInstall)
        {
            this.canInstall = canInstall;
            rerender = true;
        }

        bool isInstalling = AppInstallService.IsInstalling(Entry.Manifest.Uuid);
        if (this.isInstalling != isInstalling)
        {
            this.isInstalling = isInstalling;
            rerender = true;
        }

        bool isInstalled = libraryEntry?.Data.IsInstalled == true;
        if (this.isInstalled != isInstalled)
        {
            this.isInstalled = isInstalled;
            rerender = true;
        }

        if (rerender)
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

    private async Task LinkPlay(bool alwaysChoose = false)
    {
        if (Entry is null)
            return;
        if (linkPlayPrimary is not null && !alwaysChoose)
            return;

        await AppLinkPlayService.Play(Entry.Manifest, DialogService);
    }

    private async Task Install(bool alwaysChoose = false)
    {
        if (Entry is null)
            return;

        isStartingInstall = true;

        AppInstallService.AppInstallConfig settings = new(DialogService)
        {
            AlwaysAskInstallMethod = alwaysChoose
        };

        bool installStarted = await AppInstallService.StartInstallAsync(Entry.Manifest, settings);
        if (installStarted)
            isInstalling = true;

        isStartingInstall = false;
        StateHasChanged();
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

    public void Dispose()
    {
        AppInstallService.InstallChanged -= OnInstallChanged;
    }
}
