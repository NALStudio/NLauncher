using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps.Installing;

public abstract class InstallTask<T> : InstallTask where T : AppInstall
{
    public new BinaryAppInstall Install => (BinaryAppInstall)base.Install;

    protected InstallTask(Guid appId, AppInstall install) : base(appId, install)
    {
    }
}

public abstract class InstallTask : IDisposable
{
    public Guid AppId { get; }
    public AppInstall Install { get; }

    protected InstallTask(Guid appId, AppInstall install)
    {
        AppId = appId;
        Install = install;
    }

    public abstract bool IsStarted { get; }
    public abstract bool IsFinished { get; }

    public virtual bool IsRunning => IsStarted && !IsFinished;

    /// <summary>
    /// Whether any installation files have been modified.
    /// </summary>
    public bool IsUnsafe { get; private set; }

    public InstallProgress? Progress { get; private set; }

    public abstract event EventHandler? Started;

    /// <summary>
    /// NOTE TO IMPLEMENTERS: Do not invoke this method, use <see cref="UpdateProgress"/> instead.
    /// </summary>
    public event EventHandler<InstallProgress>? InstallProgressChanged;

    /// <summary>
    /// Fires AFTER the install has finished.
    /// </summary>
    public abstract event EventHandler<InstallResult>? Finished;

    /// <summary>
    /// Marks the current app as in an unsafe install state.
    /// </summary>
    protected void MarkUnsafe()
    {
        IsUnsafe = true;
    }

    protected void ResetProgress()
    {
        Progress = null;
    }

    protected void UpdateProgress(InstallProgress progress)
    {
        Progress = progress;
        InstallProgressChanged?.Invoke(this, progress);
    }

    public abstract ValueTask<bool> StartAsync();
    public abstract ValueTask<bool> RestartAsync();

    /// <summary>
    /// Returns <see langword="false"/> if installation isn't running.
    /// </summary>
    public abstract bool RequestCancel();

    public abstract InstallResult GetResult();
    public abstract ValueTask<InstallResult> WaitForResult(CancellationToken cancellationToken = default);

    public abstract void Dispose();
}
