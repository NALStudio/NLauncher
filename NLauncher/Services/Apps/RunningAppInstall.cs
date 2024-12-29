using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Library;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Services.Apps;

public class RunningAppInstall
{
    private readonly LibraryService library;
    private readonly IPlatformInstaller installer;

    public AppManifest App { get; }
    public AppVersion Version { get; }
    public AppInstall Install { get; }

    [MemberNotNullWhen(true, nameof(installTask))]
    public bool IsFinished => installTask?.Task.IsCompleted == true;

    private (Task<InstallResult> Task, CancellationTokenSource Cancellation)? installTask = null;
    public InstallProgress? Progress { get; set; }

    public event Action? OnRestarted;
    public event Action<InstallResult>? OnCompleted;

    public RunningAppInstall(LibraryService library, IPlatformInstaller installer, AppManifest app, AppVersion version, AppInstall install)
    {
        this.library = library;
        this.installer = installer;

        App = app;
        Version = version;
        Install = install;
    }

    public void Start()
    {
        if (installTask.HasValue)
            throw new InvalidOperationException("Install already started.");

        CancellationTokenSource ct = new();
        Task<InstallResult> task = RunInstall(ct.Token);

        // Execute synchronously so that we can update Blazor UI with the event.
        task.ContinueWith(t => OnCompleted?.Invoke(t.Result), TaskContinuationOptions.ExecuteSynchronously);

        installTask = (task, ct);
    }

    /// <summary>
    /// Returns <see langword="true"/> if restart was successful.
    /// </summary>
    public async ValueTask<bool> RestartAsync()
    {
        if (installTask.HasValue)
        {
            installTask.Value.Cancellation.Cancel();

            InstallResult result = await installTask.Value.Task;
            if (!result.IsCancelled)
                return false;

            installTask = null;
            Progress = null;
        }

        Start();

        OnRestarted?.Invoke();
        return true;
    }

    public void RequestCancel()
    {
        installTask?.Cancellation.Cancel();
    }

    public InstallResult GetResult()
    {
        if (!IsFinished)
            throw new InvalidOperationException("Installation hasn't yet finished.");

        return installTask.Value.Task.Result;
    }

    private async Task<InstallResult> RunInstall(CancellationToken cancellationToken)
    {
        InstallResult result;

        try
        {
            // Remove old install entry
            await library.UpdateEntryAsync(App.Uuid, lib => lib with { Install = null, InstalledVerNum = null });

            // Start install
            result = await installer.InstallAsync(App.Uuid, Install, onProgressUpdate: UpdateProgress, cancellationToken);

            // Add new install entry if install was successful
            if (result.IsSuccess)
                await library.UpdateEntryAsync(App.Uuid, lib => lib with { Install = Install, InstalledVerNum = Version.VerNum });
        }
        catch (OperationCanceledException)
        {
            result = InstallResult.Cancelled();
        }
        catch
        {
            result = InstallResult.Errored();
        }

        return result;
    }

    private void UpdateProgress(InstallProgress progress)
    {
        Progress = progress;
    }
}
