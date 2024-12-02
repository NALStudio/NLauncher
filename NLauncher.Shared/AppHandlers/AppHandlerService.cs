using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Base;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace NLauncher.Shared.AppHandlers;

public class AppHandlerService
{
    private readonly ImmutableArray<AppHandler> handlers;

    /// <summary>
    /// The handlers are prioritised in the order of <paramref name="handlers"/>.
    /// </summary>
    public AppHandlerService(params IEnumerable<AppHandler> handlers)
    {
        this.handlers = handlers.ToImmutableArray();
    }

    // Convert IEnumerable into ImmutableArray since I cannot guarantee that the IEnumerable can be enumerated multiple times
    public IEnumerable<AppHandler> GetSupportedHandlers(params IEnumerable<AppInstall> installs) => GetSupportedHandlers(installs.ToImmutableArray());
    public IEnumerable<AppHandler> GetSupportedHandlers(params ImmutableArray<AppInstall> installs)
    {
        foreach (AppHandler handler in handlers)
        {
            if (installs.Any(handler.CanHandle))
                yield return handler;
        }
    }
}
