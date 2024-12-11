using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Components.Dialogs.ChooseInstall;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Library;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;
using NLauncher.Shared.AppHandlers.Shared;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Components.Cards.AppInfo;

public partial class AppInfoCard
{
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

    protected override async Task OnParametersSetAsync()
    {
        canAddToLibrary = false;

        if (Entry is not null)
        {
            bool hasEntry = await LibraryService.HasEntryForApp(Entry.Manifest.Uuid);
            canAddToLibrary = !hasEntry;
            StateHasChanged();
        }
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

    private ImmutableArray<AppInstall> GetInstalls()
    {
        return Entry?.Manifest.GetLatestVersion()?.Installs ?? ImmutableArray<AppInstall>.Empty;
    }

    /// <summary>
    /// <inheritdoc cref="AppHandlerService.GetSupportedHandlers"/>
    /// </summary>
    private (ImmutableArray<AppHandler> Handlers, InstallAppHandler? PreferredHandler) GetAppHandlers()
    {
        ImmutableArray<AppInstall> installs = GetInstalls();
        if (installs.IsEmpty)
        {
            return (ImmutableArray<AppHandler>.Empty, null);
        }
        else
        {
            ImmutableArray<AppHandler> handlers = AppHandlerService.GetSupportedHandlers(installs).ToImmutableArray();
            return (handlers, (InstallAppHandler?)handlers.FirstOrDefault(static h => h is InstallAppHandler));
        }
    }

    private static string GetReleaseDateString(AppRelease appRelease)
    {
        DateOnly? release = appRelease.ReleaseDate;

        if (release.HasValue)
            return release.Value.ToShortDateString();
        else
            return "TBD";
    }

    private static string GetLinkText(LinkAppHandler handler)
    {
        if (handler is WebsiteLinkAppHandler)
            return "Play Now";
        if (handler is StoreLinkAppHandler)
            return "Open Store Page";

        // Fallback if we add a new handler and forget to add its text
        return "Open External Link";
    }

    private Platforms GetSupportedPlatforms()
    {
        ImmutableArray<AppInstall>? appInstalls = Entry?.Manifest.GetLatestVersion()?.Installs;

        Platforms supported = Platforms.None;
        foreach (AppInstall install in appInstalls ?? Enumerable.Empty<AppInstall>())
            supported |= install.GetSupportedPlatforms();

        return supported;
    }

    private async Task StartInstallOrOpenInstallDialog()
    {
        var handlerData = GetAppHandlers();

        if (handlerData.PreferredHandler is not null)
            await StartInstall(handlerData.PreferredHandler);
        else
            await OpenInstallDialog(handlerData: handlerData);
    }

    private Task StartInstall(InstallAppHandler installHandler)
    {
        Debug.WriteLine("INSTALLING NOT YET IMPLEMENTED");
        return Task.CompletedTask;
    }

    private async Task OpenInstallDialog() => await OpenInstallDialog(handlerData: GetAppHandlers());
    private async Task OpenInstallDialog((ImmutableArray<AppHandler> Handlers, InstallAppHandler? PreferredHandler) handlerData)
    {
        ImmutableArray<AppInstall> installs = GetInstalls();
        AppHandler? recommendedAppHandler = handlerData.PreferredHandler ?? handlerData.Handlers.FirstOrDefault(static h => h is RecommendNLauncherAppHandler);

        DialogParameters<ChooseInstallDialog> parameters = new()
        {
            { x => x.Handlers, handlerData.Handlers },
            { x => x.Installs, installs },
            { x => x.RecommendedHandler, recommendedAppHandler },
        };

        DialogOptions options = new()
        {
            CloseOnEscapeKey = true,
            CloseButton = true,

            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true,
        };

        await DialogService.ShowAsync<ChooseInstallDialog>("Choose Install Option", parameters, options);
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
