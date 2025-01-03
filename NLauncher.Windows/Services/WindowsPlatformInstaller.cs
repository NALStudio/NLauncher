using Microsoft.Extensions.Logging;
using NLauncher.Code.Models;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps;
using NLauncher.Windows.Models;
using System.Diagnostics;

namespace NLauncher.Windows.Services;
public class WindowsPlatformInstaller : IPlatformInstaller, IDisposable
{
    private readonly List<Process> runningProcesses = new();

    private readonly ILogger<WindowsPlatformInstaller> logger;
    public WindowsPlatformInstaller(ILogger<WindowsPlatformInstaller> logger)
    {
        this.logger = logger;
    }

    public bool InstallSupported(AppInstall install)
    {
        return install is BinaryAppInstall bai && bai.Platform.HasFlag(Platforms.Windows);
    }

    public async Task<InstallResult> InstallAsync(Guid appId, AppInstall install, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken)
    {
        InstallResult result = await RunInstallAsync(uninstall: false, appId, install, onProgressUpdate, cancellationToken);

        // Clean download directory
        DirectoryInfo downloadDir = SystemPaths.GetDownloadsPath(appId);
        if (downloadDir.Exists)
        {
            if (result.IsSuccess)
            {
                logger.LogError("Install process did not clean up the download directory.");
            }
            else
            {
                logger.LogInformation("Cleaning killed install process's download directory...");
                downloadDir.Delete(recursive: true);
            }
        }

        return result;
    }

    public async Task<InstallResult> UninstallAsync(Guid appId, AppInstall existingInstall, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken)
    {
        return await RunInstallAsync(uninstall: true, appId, existingInstall, onProgressUpdate, cancellationToken);
    }

    // We run both install and uninstall in a separate process to be able to run as administrator
    private async Task<InstallResult> RunInstallAsync(bool uninstall, Guid appId, AppInstall install, Action<InstallProgress> onProgressUpdate, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting install... (uninstall: {})", uninstall);

        // Run process
        int exitCode;
        using (Process process = CreateInstallProcess(appId, (BinaryAppInstall)install, uninstall: uninstall))
        {
            process.OutputDataReceived += (s, e) => OutputDataReceived(appId, e.Data, onProgressUpdate);

            cancellationToken.Register(process.Kill);

            // One final chance to exit before process is actually started
            if (cancellationToken.IsCancellationRequested)
                return InstallResult.Cancelled();

            runningProcesses.Add(process);

            process.Start();
            process.BeginOutputReadLine();

            await process.WaitForExitAsync(CancellationToken.None);
            runningProcesses.Remove(process);

            exitCode = process.ExitCode;
        }

        logger.LogInformation("Windows install finished with exit code: {}", exitCode);

        // Exit
        if (exitCode != 0)
        {
            if (cancellationToken.IsCancellationRequested)
                return InstallResult.Cancelled();
            else
                return InstallResult.Errored($"Process failed with exit code: {exitCode}");
        }
        else
        {
            return InstallResult.Success();
        }
    }

    private static Process CreateInstallProcess(Guid appId, BinaryAppInstall install, bool uninstall)
    {
        string[] processArgs;
        if (!uninstall)
        {
            processArgs = new string[]
            {
                "install", appId.ToString(),
                "binary", install.DownloadUrl.ToString(),
                "--hash", install.DownloadHash
            };
        }
        else
        {
            processArgs = new string[]
            {
                "uninstall", appId.ToString(), "binary"
            };
        }

        return new Process()
        {
            StartInfo = new ProcessStartInfo(Application.ExecutablePath, processArgs)
            {
                UseShellExecute = false, // we redirect stdout
                RedirectStandardOutput = true,
                Verb = "runas" // run as administrator
            }
        };
    }

    private void OutputDataReceived(Guid appId, string? line, Action<InstallProgress> onProgressUpdate)
    {
        if (line is null)
            return;
        Debug.Assert(!line.EndsWith('\n'));

        logger.LogDebug("{}: {}", appId, line);

        InstallProgress progress = ParseOutputLine(line);
        onProgressUpdate(progress);
    }

    private static InstallProgress ParseOutputLine(string line)
    {
        if (DownloadProgress.TryParse(line, out DownloadProgress p))
            return InstallProgress.Download(p.DownloadedBytes, p.TotalBytes);

        return InstallProgress.Indeterminate(line);
    }

    public void Dispose()
    {
        foreach (Process p in runningProcesses)
        {
            p.Kill();
            p.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
