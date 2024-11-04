using NLauncher.IndexManager.Commands;
using NLauncher.IndexManager.Commands.Commands;
using NLauncher.IndexManager.Commands.Commands.Main;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NLauncher.IndexManager;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Fix for emojis not working
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        CommandApp<MainCommand> app = new();

        app.Configure(config =>
        {
            config.PropagateExceptions();

            config.AddCommand<CreateCommand>("create");
            config.AddCommand<DeleteCommand>("delete");
        });

        try
        {
            return await app.RunAsync(args);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return -99;
        }
    }
}