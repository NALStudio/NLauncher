using Spectre.Console.Cli;
using System.Diagnostics;

namespace NLauncher.Windows.Commands;

internal class RunSettings : CommandSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public required Guid AppId { get; init; }

    [CommandOption("-x|--executable <REL_PATH>")]
    public required string ExePath { get; init; }
}

internal class RunCommand : Command<RunSettings>
{
    public override int Execute(CommandContext context, RunSettings settings)
    {
        string exe = settings.ExePath;

        // Do rudimentary input validation...
        // I would love to verify that the relative path doesn't escape out of the game's local files directory, but I have no idea how to do that...
        if (Path.GetExtension(exe) != ".exe")
        {
            Console.WriteLine($"Invalid file type: '{exe}'");
            return 1;
        }
        if (Path.IsPathRooted(exe))
        {
            Console.WriteLine($"Path must be relative: '{exe}'");
            return 1;
        }
        if (!File.Exists(exe))
        {
            Console.WriteLine($"Executable not found in path: '{exe}'");
            return 1;
        }

        DateTimeOffset start = DateTimeOffset.UtcNow;

        Process app = Process.Start(exe);

        // In an ideal world, we would disable the c# built-in threadpool to completely shut this application down
        // so that it doesn't use any system resources, but I don't think this is possible at the moment...

        // Wait synchronously so that the runtime is blocked from doing any other work while the game is running
        // as we want to take as little resources from the game as possible
        app.WaitForExit();

        DateTimeOffset end = DateTimeOffset.UtcNow;

        return 0;
    }
}
