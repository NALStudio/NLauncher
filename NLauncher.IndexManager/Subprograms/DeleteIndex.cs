using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Subprograms;
internal class DeleteIndex : Subprogram
{
    public override ValueTask<SubprogramError?> MainAsync(SubprogramContext ctx)
    {
        // Just so we don't accidentally purge the wrong directory
        ctx.ThrowIfNotExists();

        AnsiConsole.Write(new Rule("Delete Index").HeavyBorder());
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Markup($"Deleting: [yellow]{ctx.Paths.Directory.EscapeMarkup()}[/]...\n"));
        bool delete = AnsiConsole.Prompt(
            new ConfirmationPrompt("Are you sure you want to delete this index?")
            {
                DefaultValue = false
            }
        );

        if (delete)
        {
            AnsiConsole.WriteLine("Deleting Index...");
            Directory.Delete(ctx.Paths.Directory, recursive: true);
            AnsiConsole.Write(new Markup("[red]Index deleted.[/]\n"));
        }
        else
        {
            AnsiConsole.Write(new Markup("[yellow]Index deletion cancelled.[/]\n"));
        }

        return new((SubprogramError?)null);
    }
}
