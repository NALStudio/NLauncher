using Microsoft.AspNetCore.Components;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Pages;

namespace NLauncher.Code.Extensions;

public static class NavigationManagerExtensions
{
    public static void NavigateToApp(this NavigationManager navigationManager, IndexManifest index, AppManifest app) => NavigateToApp(navigationManager, index, app.Uuid);
    public static void NavigateToApp(this NavigationManager navigationManager, IndexManifest index, Guid appUuid)
    {
        string route = AppStorePage.GetPageRoute(index, appUuid);
        navigationManager.NavigateTo(route);
    }
}
