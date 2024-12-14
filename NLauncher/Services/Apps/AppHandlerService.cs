using NLauncher.Shared.AppHandlers.Base;
using System.Collections.Immutable;

namespace NLauncher.Services.Apps;

public class AppHandlerService
{
    public ImmutableArray<AppHandler> AllHandlers { get; }
    public ImmutableArray<AppHandler> AllHandlersExceptLink { get; }

    public ImmutableArray<LinkAppHandler> LinkAppHandlers { get; }
    public ImmutableArray<InstallAppHandler> InstallAppHandlers { get; }


    /// <summary>
    /// The handlers are prioritised in the order of <paramref name="handlers"/>.
    /// </summary>
    public AppHandlerService(params IEnumerable<AppHandler> handlers)
    {
        AllHandlers = handlers.ToImmutableArray();
        AllHandlersExceptLink = AllHandlers.Where(static h => h is not LinkAppHandler).ToImmutableArray();

        LinkAppHandlers = GetHandlersOfType<LinkAppHandler>(AllHandlers);
        InstallAppHandlers = GetHandlersOfType<InstallAppHandler>(AllHandlers);
    }

    private static ImmutableArray<T> GetHandlersOfType<T>(ImmutableArray<AppHandler> allHandlers)
    {
        return allHandlers.Where(static h => h is T)
                          .Cast<T>()
                          .ToImmutableArray();
    }
}
