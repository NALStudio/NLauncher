using Microsoft.AspNetCore.Components;
using NLauncher.Code.Extensions;
using NLauncher.Code.IndexSearch;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Pages;
using NLauncher.Services.Index;
using System.Collections.Immutable;

namespace NLauncher.Layout;

public partial class MainLayout
{
    [Inject]
    private IndexService IndexService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool settingsMenuOpen = false;
    private bool downloadsMenuOpen = false;

    private bool AnyMenuOpen => settingsMenuOpen || downloadsMenuOpen;

    private void SetMenuOpen(bool settings = false, bool downloads = false)
    {
        if (settings && downloads)
            throw new ArgumentException("Cannot have multiple menus open at once.");

        settingsMenuOpen = settings;
        downloadsMenuOpen = downloads;
    }

    private void CloseAllMenus() => SetMenuOpen();
    private void ToggleSettingsMenu() => SetMenuOpen(settings: !settingsMenuOpen);
    private void ToggleDownloadsMenu() => SetMenuOpen(downloads: !downloadsMenuOpen);

    private async Task<IEnumerable<IndexEntry>> SearchIndex(string search, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Enumerable.Empty<IndexEntry>();

        IndexManifest index = await IndexService.GetIndexAsync();
        return await Task.Run(() => new IndexSearcher(search).SearchIndex(index), cancellationToken);
    }

    private async ValueTask NavigateTo(AppManifest app)
    {
        IndexManifest index = await IndexService.GetIndexAsync();
        NavigationManager.NavigateToApp(index, app);
    }
}
