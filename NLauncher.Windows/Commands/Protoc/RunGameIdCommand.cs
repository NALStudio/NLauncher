using NLauncher.Services.Library;
using NLauncher.Windows.Services;
using NLauncher.Windows.Services.Apps;
using NLauncher.Windows.Services.Installing;
using System.Globalization;

namespace NLauncher.Windows.Commands.Protoc;
internal class RunGameIdCommand : ProtocCommand
{
    public override async Task<ProtocError?> ExecuteAsync(string[] args)
    {
        if (!(args.Length > 0 && Guid.TryParse(args[0], CultureInfo.InvariantCulture, out Guid appId)))
            return "Could not parse game id.";

        LibraryService library = new(logger: null, new WindowsStorageService());

        LibraryEntry? entry = await library.TryGetEntry(appId);
        if (!entry.HasValue)
            return "Application not found.";

        LibraryEntry e = entry.Value;

        if (!e.Data.IsInstalled)
            return "Application is not currently installed.";

        if (!WindowsPlatformInstaller.InstallExists(appId, e.Data.Install.Install))
            return "Application installation is corrupted.";

        List<string> commandArgs = ["run"];
        WindowsAppStartup.CreateRunCommandArgs(commandArgs, e.AppId, e.Data.Install.Install, e.Data.LaunchOptions);

        int result = await Program.BuildDefaultCommandApp().RunAsync(commandArgs);
        if (result != 0)
            return "Failed to run application.";

        return null;
    }
}
