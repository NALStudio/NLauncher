﻿using Microsoft.Extensions.Logging;
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

    private static void AddRunData(AppInstall install, Collection<string> args)
    {
        switch (install)
        {
            case BinaryAppInstall bai:
                args.Add("binary");
                args.Add("--executable");
                args.Add(bai.ExecutablePath);
                break;
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
        Collection<string> args = start.ArgumentList; // ProcessStartInfo.ArgumentList escapes all arguments automatically

        args.Add("command");
        args.Add("run");
        args.Add(appId.ToString());

        AddRunData(install, args);

        if (appArgs is not null)
        {
            // Escape since ProcessStartInfo doesn't seem to be able to escape this string automatically
            string appArgsEscaped = CommandLineHelpers.EscapeStringWindows(appArgs);

            args.Add("--args");
            args.Add(appArgsEscaped);
        }

        return start;
    }
}
