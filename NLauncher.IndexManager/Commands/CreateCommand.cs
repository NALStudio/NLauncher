using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands;
internal class CreateCommand : AsyncCommand<MainSettings>
{
    private static ValidationResult ValidateIndexPath(string path)
    {
        if (!path.StartsWith('/'))
            return ValidationResult.Error("Path must start with a slash.");
        if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
            return ValidationResult.Error("Path is not a valid relative path.");
        if (path.EndsWith('/'))
            return ValidationResult.Error("Path cannot end with a slash.");

        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings)
    {
        string dirpath = settings.Context.Paths.Directory;
        Debug.Assert(!DirectoryContainsFiles(dirpath));

        AnsiConsole.Write(new Rule("Create Index").HeavyBorder());
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("Repository Info").LeftJustified());
        string owner = AnsiConsole.Ask<string>("Repository Owner:");
        string repoName = AnsiConsole.Ask<string>("Repository Name:");

        string indexPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Path To Index:")
                .DefaultValue("/")
                .Validate(ValidateIndexPath)
        );

        IndexMeta meta = new()
        {
            Repository = new()
            {
                Owner = owner,
                Repo = repoName,
                Path = indexPath
            }
        };

        AnsiConsole.WriteLine("Creating Index...");
        await Create(settings.Context.Paths, meta);
        AnsiConsole.Write(new Markup("[green]Index created.[/]\n"));

        return 0;
    }

    public override ValidationResult Validate(CommandContext context, MainSettings settings)
    {
        if (DirectoryContainsFiles(settings.Context.Paths.Directory))
            return ValidationResult.Error("Cannot create index, directory contains files.");

        return ValidationResult.Success();
    }

    /// <summary>
    /// If directory doesn't exist, returns <see langword="false"/>
    /// </summary>
    private static bool DirectoryContainsFiles(string dirpath)
    {
        if (!Directory.Exists(dirpath))
            return false;

        return Directory.EnumerateFileSystemEntries(dirpath).Any();
    }

    private static async Task Create(IndexPaths paths, IndexMeta index)
    {
        // Create directory
        Directory.CreateDirectory(paths.Directory);

        // Create index.json file
        string indexJson = JsonSerializer.Serialize(index, IndexJsonSerializerContext.Default.IndexMeta);
        await File.WriteAllTextAsync(paths.IndexFile, indexJson);

        // Create url_aliases.json file
        string aliasesJson = JsonSerializer.Serialize(AppAliases.Empty, IndexJsonSerializerContext.Default.AppAliases);
        await File.WriteAllTextAsync(paths.AliasesFile, aliasesJson);
    }
}
