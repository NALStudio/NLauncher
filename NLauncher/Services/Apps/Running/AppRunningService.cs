using MudBlazor;
using NLauncher.Components.Dialogs;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Index;
using NLauncher.Services.Library;

namespace NLauncher.Services.Apps.Running;
public class AppRunningService
{
    private readonly IndexService indexService;
    private readonly LibraryService libraryService;
    private readonly IAppStartup appStartup;

    public AppRunningService(IndexService index, LibraryService library, IAppStartup appStartup)
    {
        indexService = index;
        libraryService = library;
        this.appStartup = appStartup;
    }


    private (Guid AppId, AppHandle Handle)? running = null;

    /// <summary>
    /// Returns <see langword="true"/> if app was ran succesfully, otherwise <see langword="false"/>
    /// </summary>
    public async Task<bool> RunApp(Guid appId, IDialogService dialogService)
    {
        if (running.HasValue && running.Value.Handle.IsRunning)
        {
            bool kill = await AskKillApp(dialogService, triedRunId: appId, alreadyRunningId: running.Value.AppId);
            if (!kill)
                return false;

            await running.Value.Handle.KillAsync();
        }

        LibraryInstallData? install = (await libraryService.TryGetEntry(appId))?.Data.Install;
        if (install is null)
        {
            await dialogService.ShowMessageBox("Error", "Application hasn't been installed.");
            return false;
        }

        AppHandle? handle = await appStartup.StartAsync(appId, install.Install, dialogService);
        if (handle is null)
            return false; // IAppStartup.StartAsync should've already displayed an error.

        running = (appId, handle);
        return true;
    }

    private async Task<bool> AskKillApp(IDialogService dialogService, Guid triedRunId, Guid alreadyRunningId)
    {
        AppManifest? triedRun = null;
        AppManifest? alreadyRunning = null;
        if (indexService.TryGetCachedIndex(out IndexManifest? cachedIndex))
        {
            // Only load manifest's if index is already cached, we want to show the dialog as fast as possible
            triedRun = cachedIndex.GetEntryOrNull(triedRunId)?.Manifest;
            alreadyRunning = cachedIndex.GetEntryOrNull(alreadyRunningId)?.Manifest;
        }

        return await ApplicationAlreadyRunning.ShowAsync(dialogService, triedRun: triedRun, alreadyRunning: alreadyRunning);
    }
}
