﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLauncher.Code.Language;
using NLauncher.Code.Models;
using NLauncher.Components.Dialogs.Installs;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.Library;
using NLauncher.Services.Sessions;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Components.Dialogs;
public partial class AppPropertiesDialog : IDisposable
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IAppLocalFiles LocalFiles { get; set; } = default!;

    [Inject]
    private IGameSessionService GameSessions { get; set; } = default!;

    [Inject]
    private AppInstallService AppInstall { get; set; } = default!;

    [Inject]
    private LibraryService Library { get; set; } = default!;

    [CascadingParameter]
    private IMudDialogInstance? Dialog { get; set; }

    [Parameter, EditorRequired]
    public required IndexEntry Entry { get; set; }
    public AppManifest App => Entry.Manifest;
    public Guid AppId => Entry.Manifest.Uuid;

    private LibraryEntry? libraryEntry;

    [NotNullIfNotNull(nameof(libraryEntry))]
    private AppInstall? ExistingInstall => libraryEntry?.Data.Install?.Install;

    private bool canInstall = false;

    private GameSession[] gameSessionsSorted = Array.Empty<GameSession>();

    private bool sizeLoaded = false;
    private string? size;

    [MemberNotNullWhen(true, nameof(ExistingInstall))]
    private bool CanBrowseLocalFiles => ExistingInstall is AppInstall ins && LocalFiles.OpenFileBrowserSupported(ins);

    [MemberNotNullWhen(true, nameof(ExistingInstall), nameof(libraryEntry))]
    private bool CanEditArgs => ExistingInstall is not null;

    protected override void OnInitialized()
    {
        AppInstall.OnInstallStarted += UpdateCanInstall;
        AppInstall.OnInstallFinished += UpdateCanInstall;

        UpdateCanInstall(AppId);
    }

    protected override async Task OnParametersSetAsync()
    {
        libraryEntry = await Library.TryGetEntry(AppId);
        StateHasChanged();

        await LoadGameSessions();
        StateHasChanged();

        await DeferredComputeSize(ExistingInstall);
        StateHasChanged();
    }

    private static string SanitizeCustomArgs(string args)
    {
        // Do not replace multiple spaces with a single space using regex
        // since we want to retain these spaces inside quotes
        return args.ReplaceLineEndings(" ").Trim();
    }

    private async Task UpdateArgs(string args)
    {
        string? sanitizedArgs = !string.IsNullOrWhiteSpace(args) ? SanitizeCustomArgs(args) : null;
        libraryEntry = await Library.UpdateEntryAsync(AppId, ld => ld with { LaunchOptions = sanitizedArgs });
        StateHasChanged();
    }

    private async Task LoadGameSessions()
    {
        GameSession[]? sessions = await GameSessions.LoadSessionsAsync(AppId);
        if (sessions is null)
        {
            gameSessionsSorted = Array.Empty<GameSession>();
            return;
        }

        // Only show sessions over 10 seconds long
        // Order from newest to oldest
        gameSessionsSorted = sessions.Where(static s => s.DurationMs > 10_000)
                                     .OrderByDescending(static s => s.Start)
                                     .ToArray();
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
        libraryEntry = await Library.UpdateEntryAsync(AppId, ld =>
        {
            if (ld.ChosenVerNum != version)
            {
                vernumChanged = true;
                return ld with { ChosenVerNum = version };
            }
            else
            {
                return ld;
            }
        });

        if (vernumChanged && !AppInstall.IsInstalling(AppId))
            await AppInstall.StartUpdateAsync(App, new AppInstallService.AppInstallConfig(DialogService) { VerifyIfNotLatestVersion = false });
        StateHasChanged();
    }

    /// <summary>
    /// Updates <see cref="canInstall"/> if <paramref name="appId"/> is equal to <see cref="AppId"/>
    /// </summary>
    private void UpdateCanInstall(Guid appId)
    {
        if (appId == AppId)
        {
            canInstall = !AppInstall.IsInstalling(AppId);
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task BrowseLocalFilesAsync()
    {
        if (!CanBrowseLocalFiles)
            return;

        await LocalFiles.OpenFileBrowserAsync(AppId, ExistingInstall);
    }

    private async Task DeferredComputeSize(AppInstall? existingInstall)
    {
        long? byteSize = null;
        if (existingInstall is not null)
        {
            // Add a small delay so that the user is aware that we are actually loading something
            await Task.Delay(Random.Shared.Next(750, 1250));
            byteSize = await Task.Run(() => LocalFiles.ComputeSizeInBytes(AppId, existingInstall));
        }

        size = byteSize.HasValue ? HumanizeBinary.HumanizeBytes(byteSize.Value) : null;
        sizeLoaded = true;
    }

    public static Task OpenAsync(IDialogService dialogService, IndexEntry entry)
    {
        DialogOptions options = new()
        {
            CloseButton = true,
            CloseOnEscapeKey = true,
            FullWidth = true
        };

        DialogParameters<AppPropertiesDialog> parameters = new()
        {
            { x => x.Entry, entry }
        };

        return dialogService.ShowAsync<AppPropertiesDialog>($"{entry.Manifest.DisplayName}'s Properties", parameters, options);
    }

    public void Dispose()
    {
        AppInstall.OnInstallStarted -= UpdateCanInstall;
        AppInstall.OnInstallFinished -= UpdateCanInstall;
    }
}