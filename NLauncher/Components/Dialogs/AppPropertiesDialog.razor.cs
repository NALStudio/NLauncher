using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Code.Language;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Installs;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.Library;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Components.Dialogs;
public partial class AppPropertiesDialog : IDisposable
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IAppLocalFiles LocalFiles { get; set; } = default!;

    [Inject]
    private AppInstallService AppInstall { get; set; } = default!;

    [Inject]
    private LibraryService Library { get; set; } = default!;

    [CascadingParameter]
    private IMudDialogInstance? Dialog { get; set; }

    [Parameter, EditorRequired]
    public required AppManifest App { get; set; }

    private LibraryEntry? libraryEntry;
    private AppInstall? ExistingInstall => libraryEntry?.Data.Install?.Install;

    private bool canInstall = false;

    private bool sizeLoaded = false;
    private string? size;

    [MemberNotNullWhen(true, nameof(ExistingInstall))]
    private bool CanBrowseLocalFiles => ExistingInstall is AppInstall ins && LocalFiles.OpenFileBrowserSupported(ins);

    protected override void OnInitialized()
    {
        AppInstall.OnInstallStarted += UpdateCanInstall;
        AppInstall.OnInstallFinished += UpdateCanInstall;

        UpdateCanInstall(null);
    }

    protected override async Task OnParametersSetAsync()
    {
        libraryEntry = await Library.TryGetEntry(App.Uuid);

        StateHasChanged();

        await DeferredComputeSize(ExistingInstall);
        StateHasChanged();
    }

    private async Task VerifyAndChangeVersionAsync(uint? version)
    {
        if (version.HasValue)
        {
            CancellableResult<uint?> result = await ConfirmInstallOlderVersionDialog.ShowAsync(DialogService, version.Value);
            if (result.WasCancelled) // User cancelled
                return;

            version = result.Value;
        }

        bool vernumChanged = false;
        libraryEntry = await Library.UpdateEntryAsync(App.Uuid, ld =>
        {
            if (ld.ChosenVerNum != version)
            {
                vernumChanged = true;
                return ld with { ChosenVerNum = version, };
            }
            else
            {
                return ld;
            }
        });

        if (vernumChanged && !AppInstall.IsInstalling(App.Uuid))
            await AppInstall.StartUpdateAsync(App, new AppInstallService.AppInstallConfig(DialogService) { VerifyIfNotLatestVersion = false });
        StateHasChanged();
    }

    /// <summary>
    /// Use null to force a refresh.
    /// </summary>
    private void UpdateCanInstall(RunningAppInstall? install)
    {
        if (install is null || install.App.Uuid == App.Uuid)
        {
            canInstall = !AppInstall.IsInstalling(App.Uuid);
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task BrowseLocalFilesAsync()
    {
        if (!CanBrowseLocalFiles)
            return;

        await LocalFiles.OpenFileBrowserAsync(App.Uuid, ExistingInstall);
    }

    private async Task DeferredComputeSize(AppInstall? existingInstall)
    {
        long? byteSize = null;
        if (existingInstall is not null)
        {
            // Add a small delay so that the user is aware that we are actually loading something
            await Task.Delay(Random.Shared.Next(750, 1250));
            byteSize = await Task.Run(() => LocalFiles.ComputeSizeInBytes(App.Uuid, existingInstall));
        }

        size = byteSize.HasValue ? HumanizeBinary.HumanizeBytes(byteSize.Value) : null;
        sizeLoaded = true;
    }

    public static Task OpenAsync(IDialogService dialogService, AppManifest app)
    {
        DialogOptions options = new()
        {
            CloseButton = true,
            CloseOnEscapeKey = true,
            FullWidth = true
        };

        DialogParameters<AppPropertiesDialog> parameters = new()
        {
            { x => x.App, app }
        };

        return dialogService.ShowAsync<AppPropertiesDialog>($"{app.DisplayName}'s Properties", parameters, options);
    }

    public void Dispose()
    {
        AppInstall.OnInstallStarted -= UpdateCanInstall;
        AppInstall.OnInstallFinished -= UpdateCanInstall;
    }
}