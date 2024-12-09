using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Components.Dialogs;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
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
    public IDialogService DialogService { get; init; } = default!;

    private const int iconSize = 64;
    private static readonly string iconSizePx = $"{iconSize}px";

    [Parameter, EditorRequired]
    public required IndexEntry? Entry { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [MemberNotNullWhen(false, nameof(Entry))]
    private bool IsLoading => Entry is null;

    private ImmutableArray<AppInstall> GetInstalls()
    {
        return Entry?.Manifest.GetLatestVersion()?.Installs ?? ImmutableArray<AppInstall>.Empty;
    }

    /// <summary>
    /// <inheritdoc cref="AppHandlerService.GetSupportedHandlers"/>
    /// </summary>
    private IEnumerable<AppHandler> GetAppHandlers()
    {
        ImmutableArray<AppInstall> installs = GetInstalls();
        if (installs.IsEmpty)
            return Enumerable.Empty<AppHandler>();
        else
            return AppHandlerService.GetSupportedHandlers(installs);
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

    private async Task StartInstallOrOpenInstallDialog(ImmutableArray<AppHandler> handlers, InstallAppHandler? installHandler)
    {
        if (installHandler is not null)
            await StartInstall(installHandler);
        else
            await OpenInstallDialog(handlers, preferredHandler: installHandler);
    }

    private Task StartInstall(InstallAppHandler installHandler)
    {
        Debug.WriteLine("INSTALLING NOT YET IMPLEMENTED");
        return Task.CompletedTask;
    }

    private async Task OpenInstallDialog(ImmutableArray<AppHandler> handlers, AppHandler? preferredHandler)
    {
        ImmutableArray<AppInstall> installs = GetInstalls();

        DialogParameters<ChooseInstallDialog> parameters = new()
        {
            { x => x.Handlers, handlers },
            { x => x.Installs, installs },
            { x => x.PreferredHandler, preferredHandler },
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
