﻿using Microsoft.Extensions.Logging;
using MudBlazor;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Uninstalls;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Library;

namespace NLauncher.Services.Apps.Installing;
public class AppUninstallService
{
    private readonly ILogger<AppUninstallService> logger;
    private readonly IPlatformInstaller installer;
    private readonly LibraryService library;
    public AppUninstallService(ILogger<AppUninstallService> logger, IPlatformInstaller installer, LibraryService library)
    {
        this.logger = logger;
        this.installer = installer;
        this.library = library;
    }

    public async Task<bool> CanUninstall(Guid appId) => (await ResolveInstall(appId)).IsSuccess;

    /// <summary>
    /// Returns <see langword="true"/> if uninstall was successul, otherwise <see langword="false"/>
    /// </summary>
    public async Task<bool> UninstallAsync(Guid appId, IDialogService dialogService)
    {
        DialogOptions options = new()
        {
            BackdropClick = false,
            CloseButton = false,
            CloseOnEscapeKey = false,

            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        DialogParameters<UninstallDialog> parameters = new()
        {
            { x => x.AppId, appId }
        };

        // Set initial title, title will be updated asynchronously as it requires an app manifest load.
        IDialogReference dialogRef = await dialogService.ShowAsync<UninstallDialog>(UninstallDialog.ConstructTitle(null), parameters, options);
        UninstallDialog? dialog = (UninstallDialog?)dialogRef.Dialog;
        await dialog!.TryLoadTitleAsync();

        InstallResult result = await InternalUninstall(appId, onProgressUpdate: dialog!.UpdateProgress);
        dialog.SetResult(result);

        await dialogRef.Result; // Wait for dialog to close
        return result.IsSuccess;
    }

    private async Task<InstallResult<AppInstall>> ResolveInstall(Guid appId)
    {
        LibraryEntry? entry = await library.TryGetEntry(appId);
        AppInstall? install = entry?.Data.Install?.Install;

        if (install is null)
            return InstallResult.Errored<AppInstall>("App not installed.");

        if (!installer.UninstallSupported(install))
            return InstallResult.Errored<AppInstall>("Uninstall not supported.");

        return InstallResult.Success(install);
    }

    private async Task<InstallResult> InternalUninstall(Guid appId, Action<InstallProgress> onProgressUpdate)
    {
        InstallResult<AppInstall> install = await ResolveInstall(appId);
        if (!install.IsSuccess)
            return InstallResult.Failed(install);

        await library.UpdateEntryAsync(appId, data => data with { Install = null });
        InstallResult result = await installer.UninstallAsync(appId, install.Value, onProgressUpdate, CancellationToken.None);

        bool removed = await library.RemoveEntryAsync(appId);
        if (!removed)
            logger.LogError("App could not be removed from library after uninstall.");

        return result;
    }
}
