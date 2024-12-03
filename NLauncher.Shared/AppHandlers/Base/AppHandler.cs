using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Shared;
using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class AppHandler
{
    public abstract bool CanHandle(AppInstall install);

    // Handlers had to be inlined inside the collection expression.
    // The app crashes if I use the spread (..) operator.

    /// <summary>
    /// Handlers are ordered by priority where the first element of the array has the highest priority.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public static readonly ImmutableArray<AppHandler> WebHandlers = [
        // Links
        new StoreLinkAppHandler(),
        new WebsiteLinkAppHandler(),

        // Manual installs
        new BinaryManualInstallAppHandler()
    ];

    /// <summary>
    /// <inheritdoc cref="WebHandlers"/>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static readonly ImmutableArray<AppHandler> WindowsHandlers = [
        // Links
        new StoreLinkAppHandler(),
        new WebsiteLinkAppHandler(),

        // Manual installs
        new BinaryManualInstallAppHandler()
    ];
}
