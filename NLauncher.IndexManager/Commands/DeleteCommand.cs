using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands;
internal class DeleteCommand : Command<MainSettings>, IMainCommand
{
    public override int Execute(CommandContext context, MainSettings settings) => Execute(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);

    public Task<int> ExecuteAsync(MainSettings settings) => Task.FromResult(Execute(settings));
    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }

    private static int Execute(MainSettings settings)
    {
        // Just so we don't accidentally purge the wrong directory
        settings.Context.ThrowIfIndexNotExists();
        string directory = settings.Context.Paths.Directory;

        AnsiFormatter.WriteHeader("Delete Index");

        AnsiConsole.Write("Deleting: ");
        AnsiConsole.Write(AnsiFormatter.DeletedFile(directory));
        AnsiConsole.WriteLine();

        bool delete = AnsiConsole.Confirm("Are you sure you want to delete this index?", defaultValue: false);
        if (!delete)
        {
            AnsiFormatter.WriteOperationCancelled();
            return 0;
        }

        AnsiFormatter.WriteSectionTitle("Delete Log");
        AnsiConsole.WriteLine("Deleting Index...");
        AnsiConsole.WriteLine();

        // Create tree manually before delete since listening to changes doesn't seem to work
        // if the directory that we listen is deleted.
        FileChangeTreeNode fileChanges = FileChangeTree.ApplyToDirectory(directory, FileChange.Deleted);

        Directory.Delete(directory, recursive: true);

        AnsiConsole.Write(FileChangeTree.Render(fileChanges));

        return 0;
    }
}
