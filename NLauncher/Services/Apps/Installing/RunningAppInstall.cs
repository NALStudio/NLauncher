using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Library;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace NLauncher.Services.Apps.Installing;

public class RunningAppInstall
{
    private readonly record struct InstallTask(Task<InstallResult> Task, CancellationTokenSource Cancellation, ChannelReader<InstallProgress> ProgressChannel);

    private readonly LibraryService library;
    private readonly IPlatformInstaller installer;

    public AppManifest App { get; }
    public AppVersion Version { get; }
    public AppInstall Install { get; }

    [MemberNotNullWhen(true, nameof(installTask))]
    public bool IsRunning => installTask?.Task.IsCompleted == false;

    [MemberNotNullWhen(true, nameof(installTask))]
    public bool IsFinished => installTask?.Task.IsCompleted == true;

    public InstallProgress? LatestProgress { get; private set; }
    private InstallTask? installTask = null;

    public event Action<RunningAppInstall>? OnStarted;

    /// <summary>
    /// Fires after the install has finished.
    /// </summary>
    /// <remarks>
    /// This function will almost always be called from another thread.
    /// </remarks>
    public event Action<RunningAppInstall>? OnFinished;

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

        // We cannot guarantee single reader as we expose the API publically
        Channel<InstallProgress> channel = Channel.CreateBounded<InstallProgress>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest, SingleWriter = true });

        CancellationTokenSource ct = new();
        Task<InstallResult> task = Task.Run(() => RunInstall(channel.Writer, ct.Token));
        task.ContinueWith(_ => OnFinished?.Invoke(this));

        installTask = new(task, ct, channel.Reader);

        OnStarted?.Invoke(this);
    }

    /// <summary>
    /// Returns <see langword="false"/> if restart was unsuccessful. (The install finished successfully before we had time to restart)
    /// </summary>
    public async ValueTask<bool> RestartAsync()
    {
        if (installTask.HasValue)
        {
            installTask.Value.Cancellation.Cancel();

            InstallResult result = await installTask.Value.Task;
            // Use result.IsSuccess instead of !result.IsCancelled so that we can restart an errored install as well
            if (result.IsSuccess)
                return false;

            installTask = null;
            LatestProgress = null;
        }

        Start();

        return true;
    }

    public void RequestCancel()
    {
        installTask?.Cancellation.Cancel();
    }

    public IAsyncEnumerable<InstallProgress> ListenForProgressUpdates(CancellationToken cancellationToken = default)
    {
        ThrowIfNotRunning();
        return installTask.Value.ProgressChannel.ReadAllAsync(cancellationToken);
    }

    public InstallResult GetResult()
    {
        ThrowIfNotFinished();
        return installTask.Value.Task.Result;
    }

    public async ValueTask<InstallResult> WaitForResult(CancellationToken cancellationToken = default)
    {
        if (IsFinished)
        {
            return GetResult();
        }
        else
        {
            ThrowIfNotRunning();
            return await installTask.Value.Task.WaitAsync(cancellationToken);
        }
    }

    /// <summary>
    /// NOTE: This function does not throw when <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    private async Task<InstallResult> RunInstall(ChannelWriter<InstallProgress> progressChannel, CancellationToken cancellationToken)
    {
        void ProgressUpdate(InstallProgress prog)
        {
            LatestProgress = prog;
            bool written = progressChannel.TryWrite(prog);
            Debug.Assert(written);
        }

        InstallResult result;

        try
        {
            // Remove old install entry
            await library.UpdateEntryAsync(App.Uuid, lib => lib with { Install = null });

            // Start install
            result = await installer.InstallAsync(App.Uuid, Install, onProgressUpdate: ProgressUpdate, cancellationToken);

            // Add new install entry if install was successful
            if (result.IsSuccess)
                await library.UpdateEntryAsync(App.Uuid, lib => lib with { Install = new(Version.VerNum, Install) });
        }
        catch (OperationCanceledException)
        {
            result = InstallResult.Cancelled();
        }
        catch
        {
            result = InstallResult.Errored();
        }

        // No need to register to CancellationToken as we catch the OperationCanceledException
        progressChannel.Complete();

        return result;
    }

    [MemberNotNull(nameof(installTask))]
    private void ThrowIfNotFinished()
    {
        if (!IsFinished)
            throw new InvalidOperationException("Installation hasn't yet finished.");
    }

    [MemberNotNull(nameof(installTask))]
    private void ThrowIfNotRunning()
    {
        if (!IsRunning)
            throw new InvalidOperationException("Installation is not running.");
    }
}
