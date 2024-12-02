using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Shared;
using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class AppHandler
{
    public abstract bool CanHandle(AppInstall install);

    /// <summary>
    /// Handlers are ordered by priority where the first element of the array has the highest priority.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public static readonly ImmutableArray<AppHandler> WebHandlers = [
        ..sharedHandlers
    ];

    /// <summary>
    /// <inheritdoc cref="WebHandlers"/>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static readonly ImmutableArray<AppHandler> WindowsHandlers = [
        ..sharedHandlers
    ];

    private static readonly ImmutableArray<AppHandler> sharedHandlers = [
        // Links
        new StoreLinkAppHandler(),
        new WebsiteLinkAppHandler(),

        // Manual installs
        new BinaryManualInstallAppHandler()
    ];
}
