using Microsoft.Extensions.Logging;
using MudBlazor;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps.Running;
using System.Diagnostics;

namespace NLauncher.Windows.Services.Apps;
public class WindowsAppStartup : IAppStartup
{
    private readonly ILogger<WindowsAppStartup> logger;
    public WindowsAppStartup(ILogger<WindowsAppStartup> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<AppHandle?> StartAsync(Guid appId, AppInstall install, IDialogService dialogService)
    {
        Process process = CreateProcess(appId, install);

        bool started = process.Start();
        if (!started)
        {
            await ShowStartFailedMessageBox(dialogService);
            logger.LogError("Process start failed.");
            return null;
        }

        try
        {
            // Wait for 1 second for the startup task to end with an error.
            // if no errors were detected during this time, the app started succesfully.
            await process.WaitForExitAsync(new CancellationTokenSource(1000).Token);
        }
        catch (OperationCanceledException)
        {
        }

        if (process.HasExited && process.ExitCode != 0)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                string output = await process.StandardOutput.ReadToEndAsync();
                logger.Log(LogLevel.Error, "Startup process failed with output:\n{}", output);
            }

            await ShowStartFailedMessageBox(dialogService);
            return null;
        }

        return new WindowsAppHandle(process, startedSuccesfully: started);
    }

    private static async Task ShowStartFailedMessageBox(IDialogService dialogService)
    {
        _ = await dialogService.ShowMessageBox("Internal Error", "Application could not be started.");
    }

    private static string[]? CreateArgs(AppInstall install)
    {
        return install switch
        {
            BinaryAppInstall bai => new string[] { "binary", "--executable", bai.ExecutablePath },
            _ => null,
        };
    }

    private static Process CreateProcess(Guid appId, AppInstall install)
    {
        string[] args = [
            "run",
            appId.ToString(),
            ..CreateArgs(install)
        ];

        return new Process()
        {
            StartInfo = new ProcessStartInfo(Application.ExecutablePath, args)
            {
                UseShellExecute = false, // we redirect stdout
                RedirectStandardOutput = true

                // probably not needed...?
                // Verb = "runas"
            }
        };
    }
}
