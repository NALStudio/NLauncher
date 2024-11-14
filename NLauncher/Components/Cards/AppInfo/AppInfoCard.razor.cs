using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;
using NLauncher.Shared.AppHandlers.Shared;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Components.Cards.AppInfo;

public partial class AppInfoCard
{
    [Inject]
    public AppHandlerService AppHandlerService { get; init; } = default!;

    private const int iconSize = 64;
    private static readonly string iconSizePx = $"{iconSize}px";

    [Parameter, EditorRequired]
    public required IndexEntry? Entry { get; set; }

    [MemberNotNullWhen(false, nameof(Entry))]
    private bool IsLoading => Entry is null;

    private IEnumerable<AppHandler> GetAppHandlers()
    {
        AppVersion? latest = Entry?.Manifest.GetLatestVersion();
        if (latest is null)
            return Enumerable.Empty<AppHandler>();

        return AppHandlerService.GetSupportedHandlers(latest.Installs);
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

    public string? GetPlatformIcon(Platforms platform)
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
