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

    /// <summary>
    /// Handlers are ordered by priority where the first element of the enumerable has the highest priority.
    /// </summary>
    public IEnumerable<AppHandler> GetSupportedHandlers(params ImmutableArray<AppInstall> installs)
    {
        foreach (AppHandler handler in handlers)
        {
            if (installs.Any(handler.CanHandle))
                yield return handler;
        }
    }
}
