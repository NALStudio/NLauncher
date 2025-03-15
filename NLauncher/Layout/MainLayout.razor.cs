using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Code.Extensions;
using NLauncher.Code.IndexSearch;
using NLauncher.Index.Models.Index;
using NLauncher.Services;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.CheckUpdate;
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

    [Inject]
    private ICheckUpdate CheckUpdate { get; set; } = default!;


    private MudAutocomplete<IndexEntry>? indexSearch;

    private bool anyInstallsRunning = false;
    private AvailableUpdate? update = null;

    private bool drawerOpen = true;

    protected override async Task OnInitializedAsync()
    {
        AppBarMenus.OnChanged += AppBarMenusChanged;
        AppInstallService.OnActiveCountChanged += InstallCountChanged;

        AvailableUpdate? update = await CheckUpdate.CheckForUpdateAsync();
        if (update is not null)
        {
            this.update = update;
            StateHasChanged();
        }
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

    private async Task NavigateTo(IndexEntry? entry)
    {
        if (entry is null)
            return;

        IndexManifest index = await IndexService.GetIndexAsync();
        NavigationManager.NavigateToApp(index, entry.Manifest);

        await indexSearch!.BlurAsync();
        await indexSearch!.ClearAsync();
    }

    private void InstallCountChanged(int count)
    {
        anyInstallsRunning = count > 0;
        InvokeAsync(StateHasChanged);
    }

    private void AppBarMenusChanged()
    {
        StateHasChanged();
    }

    public void Dispose()
    {
        AppBarMenus.OnChanged -= AppBarMenusChanged;
        AppInstallService.OnActiveCountChanged -= InstallCountChanged;

        GC.SuppressFinalize(this);
    }
}
