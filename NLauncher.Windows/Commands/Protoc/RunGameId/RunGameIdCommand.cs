using NLauncher.Services.Library;
using NLauncher.Windows.Services;
using NLauncher.Windows.Services.Apps;
using NLauncher.Windows.Services.Installing;
using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Protoc.RunGameId;
internal class RunGameIdCommand : AsyncCommand<RunGameIdSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RunGameIdSettings settings)
    {
        try
        {
            return await InternalExecuteAsync(settings);
        }
        catch
        {
            return Error("An internal error occured");
        }
    }

    private static int Error(string message, string title = "Failed To Start Application")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return 0; // Return 0 so we don't display the default error message
    }

    private static async Task<int> InternalExecuteAsync(RunGameIdSettings settings)
    {
        LibraryService library = new(logger: null, new WindowsStorageService());

        LibraryEntry? entry = library.TryGetEntry(settings.AppId).Result;
        if (!entry.HasValue)
            return Error("Application not found.");

        LibraryEntry e = entry.Value;

        if (!e.Data.IsInstalled)
            return Error("Application is not currently installed.");

        if (!WindowsPlatformInstaller.InstallExists(settings.AppId, e.Data.Install.Install))
            return Error("Application installation is corrupted.");

        List<string> commandArgs = ["run"];
        WindowsAppStartup.CreateRunCommandArgs(commandArgs, e.AppId, e.Data.Install.Install, e.Data.LaunchOptions);

        return await Program.BuildDefaultCommandApp().RunAsync(commandArgs);
    }
}
