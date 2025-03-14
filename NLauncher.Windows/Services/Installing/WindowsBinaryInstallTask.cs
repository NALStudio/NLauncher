using Microsoft.Extensions.Logging;
using NLauncher.Code.Models;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps.Installing;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

namespace NLauncher.Windows.Services.Installing;
internal class WindowsBinaryInstallTask : InstallTask<BinaryAppInstall>
{
    private readonly ILogger<WindowsBinaryInstallTask> logger;
    private readonly bool uninstall;
    private readonly string pipeId;

    public WindowsBinaryInstallTask(ILogger<WindowsBinaryInstallTask> logger, Guid appId, BinaryAppInstall install, bool uninstall) : base(appId, install)
    {
        this.logger = logger;

        pipeId = $"nlauncher_install_{appId}";
        this.uninstall = uninstall;
    }

    private NamedPipeServerStream? pipe;
    private PipeProgressReader? pipeReader;

    private Process? process;

    private bool cancelled;
    private bool cancelling;
    private InstallResult? result;

    public override bool IsStarted => process is not null || IsFinished;

    [MemberNotNullWhen(true, nameof(result))]
    public override bool IsFinished => result.HasValue;

    public override event EventHandler? Started;
    public override event EventHandler<InstallResult>? Finished;

    public override void Dispose()
    {
        RequestCancel();

        pipeReader?.Dispose();
        pipe?.Dispose();
        process?.Dispose();
    }

    public override InstallResult GetResult()
    {
        if (!IsFinished)
            throw new InvalidOperationException("Application hasn't finished.");
        return result.Value;
    }

    public override bool RequestCancel()
    {
        if (process is not null)
        {
            cancelling = true;
            bool? cancelled = null;
            try
            {
                cancelled = TryKillProcessAndChildren(process);
            }
            finally
            {
                this.cancelled = cancelled ?? false;
                cancelling = false;
            }

            return cancelled.Value;
        }
        else
        {
            return false;
        }
    }

    private void Reset()
    {
        process = null;
        cancelled = false;
        result = null;
        ResetProgress();
    }

    private void ProcessExited(object? sender, EventArgs args)
    {
        Debug.Assert(ReferenceEquals(sender, process));

        InstallResult result;
        int exitCode = process!.ExitCode;
        if (exitCode == 0)
        {
            result = InstallResult.Success();
        }
        else
        {
            if (cancelling || cancelled)
                result = InstallResult.Cancelled();
            else
                result = InstallResult.Errored($"Process failed with exit code: {exitCode}");
        }
        this.result = result;

        ExitCleanup();

        Finished?.Invoke(this, GetResult());
    }

    private void ExitCleanup()
    {
        process!.Dispose();
        process = null;
        pipeReader!.Dispose();
        pipeReader = null;
        pipe!.Dispose();
        pipe = null;
    }

    public override async ValueTask<bool> RestartAsync()
    {
        if (IsStarted)
        {
            RequestCancel();
            InstallResult result = await WaitForResult();
            if (result.IsSuccess) // Only allow restart on cancelled/failed results
                return false;
        }

        Reset();
        return await StartAsync();
    }

    public override async ValueTask<bool> StartAsync()
    {
        if (IsStarted)
            throw new InvalidOperationException("Install already started.");

        // Reuse pipe during restart
        Debug.Assert(pipe is null);
        pipe = new(pipeId, PipeDirection.In, maxNumberOfServerInstances: 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

        Debug.Assert(pipeReader is null);
        pipeReader = new PipeProgressReader(pipe, UpdateProgress);

        process = CreateProcess(pipeId);
        process.EnableRaisingEvents = true;
        process.Exited += ProcessExited;

        bool started;
        try
        {
            process.Start();

            // Wait 3 seconds for the connection, if no connect during this time, argument parsing failed.
            await pipe.WaitForConnectionAsync(new CancellationTokenSource(3000).Token);
            pipeReader.Start(); // Only start reading once client has connected

            started = true;
        }
        catch (Win32Exception)
        {
            started = false;
        }
        catch (OperationCanceledException)
        {
            RequestCancel();
            started = false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Install startup failed.");
            RequestCancel();
            started = false;
        }

        if (started)
        {
            MarkUnsafe();
            Started?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ExitCleanup();
        }

        return started;
    }

    public override async ValueTask<InstallResult> WaitForResult(CancellationToken cancellationToken = default)
    {
        if (!IsStarted)
            throw new InvalidOperationException("Application not started.");

        if (IsFinished)
            return GetResult();

        Debug.Assert(process is not null);
        await process.WaitForExitAsync(cancellationToken);

        return GetResult();
    }

    private static bool TryKillProcessAndChildren(Process p)
    {
        try
        {
            p.Kill(entireProcessTree: true);
            return true;
        }
        catch (InvalidOperationException)
        {
            // App has already exited.
            // There is no reliable way to check this other than catching the exception.
            return false;
        }
    }

    private Process CreateProcess(string pipeId)
    {
        List<string> processArgs;
        if (!uninstall)
        {
            processArgs =
            [
                "install", AppId.ToString(),
                "--pipe", pipeId,
                "binary", Install.DownloadUrl.ToString(),
                "--hash", Install.DownloadHash
            ];
        }
        else
        {
            processArgs =
            [
                "uninstall", AppId.ToString(),
                "--pipe", pipeId,
                "binary"
            ];
        }

        processArgs.Insert(0, "command");

        return new Process()
        {
            StartInfo = new ProcessStartInfo(Application.ExecutablePath, processArgs)
            {
                UseShellExecute = true, // needed for runas
                                        // RedirectStandardOutput = true, Cannot redirect with UseShellExecute
                Verb = "runas" // run as administrator
            }
        };
    }
}
