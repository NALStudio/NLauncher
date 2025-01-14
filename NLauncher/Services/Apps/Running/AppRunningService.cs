using MudBlazor;
using NLauncher.Components.Dialogs;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Index;
using NLauncher.Services.Library;
using System.Diagnostics.CodeAnalysis;

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

    [MemberNotNullWhen(true, nameof(running))]
    private bool AnyIsRunning => running.HasValue && running.Value.Handle.IsRunning;

    public bool IsRunning(Guid appId) => AnyIsRunning && running.Value.AppId == appId;

    /// <summary>
    /// Returns <see langword="true"/> if app was ran succesfully, otherwise <see langword="false"/>
    /// </summary>
    public async Task<bool> RunApp(Guid appId, IDialogService dialogService)
    {
        if (AnyIsRunning)
        {
            bool killed = await AppAlreadyRunning(dialogService, triedRunId: appId, alreadyRunningId: running.Value.AppId, alreadyRunningHandle: running.Value.Handle);
            if (!killed)
                return false;
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

    /// <summary>
    /// Returns <see langword="true"/> if application was killed succesfully, <see langword="false"/> otherwise.
    /// </summary>
    public async Task<bool> KillRunningApp(IDialogService dialogService)
    {
        if (!AnyIsRunning)
        {
            await dialogService.ShowMessageBox("Error!", "No app is currently running.");
            return false;
        }

        AppManifest? app = null;
        if (indexService.TryGetCachedIndex(out IndexManifest? manifest))
            app = manifest.GetEntryOrNull(running.Value.AppId)?.Manifest;

        return await AskKillRunningApp(dialogService, app, running.Value.Handle);
    }

    private static async Task<bool> AskKillRunningApp(IDialogService dialogService, AppManifest? app, AppHandle appHandle)
    {
        bool kill = await ConfirmApplicationKill.ShowAsync(dialogService, app);
        if (kill)
            await appHandle.KillAsync();
        return kill;
    }

    private async Task<bool> AppAlreadyRunning(IDialogService dialogService, Guid triedRunId, Guid alreadyRunningId, AppHandle alreadyRunningHandle)
    {
        AppManifest? triedRun = null;
        AppManifest? alreadyRunning = null;
        if (indexService.TryGetCachedIndex(out IndexManifest? cachedIndex))
        {
            // Only load manifest's if index is already cached, we want to show the dialog as fast as possible
            triedRun = cachedIndex.GetEntryOrNull(triedRunId)?.Manifest;
            alreadyRunning = cachedIndex.GetEntryOrNull(alreadyRunningId)?.Manifest;
        }

        bool kill = await ApplicationAlreadyRunning.ShowAsync(dialogService, triedRun: triedRun, alreadyRunning: alreadyRunning);
        if (kill)
            return await AskKillRunningApp(dialogService, alreadyRunning, alreadyRunningHandle);
        else
            return false;
    }
}
