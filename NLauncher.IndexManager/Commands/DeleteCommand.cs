using NLauncher.IndexManager.Commands.Main;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands;
internal class DeleteCommand : Command<MainSettings>
{
    public override int Execute(CommandContext context, MainSettings settings)
    {
        // Just so we don't accidentally purge the wrong directory
        settings.Context.ThrowIfIndexNotExists();
        string directory = settings.Context.Paths.Directory;

        AnsiConsole.Write(new Rule("Delete Index").HeavyBorder());
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Markup($"Deleting: [yellow]{directory.EscapeMarkup()}[/]...\n"));
        bool delete = AnsiConsole.Confirm("Are you sure you want to delete this index?", defaultValue: false);

        if (delete)
        {
            AnsiConsole.WriteLine("Deleting Index...");
            Directory.Delete(directory, recursive: true);
            AnsiConsole.Write(new Markup("[red]Index deleted.[/]\n"));
        }
        else
        {
            AnsiConsole.Write(new Markup("[yellow]Index deletion cancelled.[/]\n"));
        }

        return 0;
    }
}
