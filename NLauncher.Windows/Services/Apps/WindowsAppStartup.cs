using Microsoft.Extensions.Logging;
using MudBlazor;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps.Running;
using NLauncher.Windows.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NLauncher.Windows.Services.Apps;
public class WindowsAppStartup : IAppStartup
{
    private readonly ILogger<WindowsAppStartup> logger;
    public WindowsAppStartup(ILogger<WindowsAppStartup> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<AppHandle?> StartAsync(Guid appId, AppInstall install, string? args, IDialogService dialogService)
    {
        ProcessStartInfo start = CreateProcessStart(appId, install, args);

        Process? process = Process.Start(start);
        if (process is null)
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

        return new WindowsAppHandle(process);
    }

    private static async Task ShowStartFailedMessageBox(IDialogService dialogService)
    {
        _ = await dialogService.ShowMessageBox("Internal Error", "Application could not be started.");
    }

    public static void CreateRunCommandArgs(ICollection<string> args, Guid appId, AppInstall install, string? appArgs, bool escapeAppArgs = false)
    {
        args.Add(appId.ToString());

        switch (install)
        {
            case BinaryAppInstall bai:
                args.Add("binary");
                args.Add("--executable");
                args.Add(bai.ExecutablePath);
                break;
        }

        if (appArgs is not null)
        {
            args.Add("--args");
            if (escapeAppArgs)
                args.Add(CommandLineHelpers.EscapeStringWindows(appArgs));
            else
                args.Add(appArgs);
        }
    }

    private static ProcessStartInfo CreateProcessStart(Guid appId, AppInstall install, string? appArgs)
    {
        ProcessStartInfo start = new(Application.ExecutablePath)
        {
            UseShellExecute = false, // we redirect stdout
            RedirectStandardOutput = true

            // probably not needed...?
            // Verb = "runas"
        };

        Collection<string> args = start.ArgumentList;
        args.Add("command");
        args.Add("run");
        // Escape since ProcessStartInfo doesn't seem to be able to escape this string automatically
        CreateRunCommandArgs(args, appId, install, appArgs, escapeAppArgs: true);

        return start;
    }
}
