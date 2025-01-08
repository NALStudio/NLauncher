using NLauncher.Services.Sessions;
using NLauncher.Windows.Services.GameSessions;
using Spectre.Console.Cli;
using System.Diagnostics;

namespace NLauncher.Windows.Commands.Run;

internal class BinaryRunSettings : RunSettings
{
    [CommandOption("-x|--executable <REL_PATH>")]
    public required string ExePath { get; init; }
}

internal class BinaryRunCommand : Command<BinaryRunSettings>
{
    public override int Execute(CommandContext context, BinaryRunSettings settings)
    {
        string exeRelative = settings.ExePath;
        string libraryPath = SystemDirectories.GetLibraryPath(settings.AppId).FullName;

        // Do rudimentary input validation...
        // I would love to verify that the relative path doesn't escape out of the game's local files directory, but I have no idea how to do that...
        if (Path.GetExtension(exeRelative) != ".exe")
        {
            Console.WriteLine($"Invalid file type: '{exeRelative}'");
            return 1;
        }
        if (Path.IsPathRooted(exeRelative))
        {
            Console.WriteLine($"Path must be relative: '{exeRelative}'");
            return 1;
        }

        if (WindowsGameSessionService.TryStartSession(settings.AppId, out GameSessionHandle? handle))
        {
            try
            {
                GameSession? session = TryRunProcessAndWait(libraryPath, exeRelative);
                if (session is null)
                {
                    Console.WriteLine("Process could not be started.");
                    return 1;
                }

                handle.WriteSessionSynchronous(session);
            }
            finally
            {
                handle.Dispose();
            }
        }
        else
        {
            Console.WriteLine("Another instance of this application is already running.");
            return 1;
        }

        return 0;
    }

    private static ProcessStartInfo CreateProcessStartInfo(string libraryPath, string exePath)
    {
        string exeFullPath = Path.Join(libraryPath, exePath);
        return new ProcessStartInfo(exeFullPath)
        {
            WorkingDirectory = libraryPath
        };
    }

    private static GameSession? TryRunProcessAndWait(string libraryPath, string exePath)
    {
        ProcessStartInfo startInfo = CreateProcessStartInfo(libraryPath, exePath);

        DateTimeOffset start = DateTimeOffset.UtcNow;
        Process? app = Process.Start(startInfo);
        if (app is null)
            return null;

        // In an ideal world, we would disable the c# built-in threadpool to completely shut this application down
        // so that it doesn't use any system resources, but I don't think this is possible at the moment...

        // Try to release as much memory as possible
        // Impact is small, but still noticeable (8,1 MB to 7,9 MB), this will probably make more of a difference with NativeAOT
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);

        // Wait synchronously so that the runtime is blocked from doing any other work while the game is running
        // as we want to take as little resources from the game as possible
        app.WaitForExit();

        DateTimeOffset end = DateTimeOffset.UtcNow;

        return new GameSession(start, end);
    }
}
