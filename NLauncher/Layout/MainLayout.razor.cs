using Microsoft.AspNetCore.Components;
using NLauncher.Code.Extensions;
using NLauncher.Code.IndexSearch;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services;
using NLauncher.Services.Apps;
using NLauncher.Services.Index;

namespace NLauncher.Layout;

public partial class MainLayout : IDisposable
{
    [Inject]
    private IndexService IndexService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AppBarMenus AppBarMenus { get; set; } = default!;

    [Inject]
    private AppInstallService AppInstallService { get; set; } = default!;

    private bool anyInstallsRunning = false;
    private bool drawerOpen = true;

    protected override void OnInitialized()
    {
        AppBarMenus.OnChanged += AppBarMenusChanged;
        AppInstallService.InstallChanged += InstallCountChanged;
    }

    private void SetDrawerOpen(bool open)
    {
        drawerOpen = open;
    }

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

    private void InstallCountChanged()
    {
        anyInstallsRunning = AppInstallService.GetInstalls().Any(static ins => !ins.IsFinished);
        StateHasChanged();
    }

    private void AppBarMenusChanged()
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        AppBarMenus.OnChanged -= AppBarMenusChanged;
        AppInstallService.InstallChanged -= InstallCountChanged;

        GC.SuppressFinalize(this);
    }
}
