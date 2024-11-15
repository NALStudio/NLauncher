using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using NLauncher.IndexManager.Components.Paths;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Commands;
internal class CreateCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        string dirpath = settings.Context.Paths.Directory;
        Debug.Assert(!DirectoryContainsFiles(dirpath));

        AnsiFormatter.WriteHeader("Create Index");

        AnsiFormatter.WriteSectionTitle("Repository Info");
        string owner = AnsiConsole.Ask<string>("Repository Owner:");
        string repoName = AnsiConsole.Ask<string>("Repository Name:");
        string branch = AnsiConsole.Ask("Repository Branch:", "main");

        string indexPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Path To Index:")
                .DefaultValue("/")
                .Validate(ValidateIndexPath)
        );

        IndexMeta meta = new()
        {
            IndexManifestPath = "./indexmanifest.json",
            Repository = new()
            {
                Owner = owner,
                Repo = repoName,
                Path = indexPath,
                Branch = branch
            }
        };

        AnsiFormatter.WriteSectionTitle("Create Log");

        AnsiConsole.WriteLine("Creating Index...");
        AnsiConsole.WriteLine();

        // We need to create the directory first, FileSystemWatcher cannot watch a non-existent directory.
        FileChange? rootChange = null;
        if (!Directory.Exists(settings.Context.Paths.Directory))
        {
            Directory.CreateDirectory(settings.Context.Paths.Directory);
            rootChange = FileChange.Created;
        }

        using (FileChangeTree.ListenAndWrite(settings.Context.Paths.Directory, rootChange: rootChange))
        {
            await Create(settings.Context.Paths, meta);
        }

        return 0;
    }

    public ValidationResult Validate(MainSettings settings)
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

    private static async Task Create(IndexPaths paths, IndexMeta index)
    {
        // Create index.json file
        string indexJson = IndexJsonSerializer.Serialize(index, IndexSerializationOptions.HumanReadable);
        await File.WriteAllTextAsync(paths.IndexFile, indexJson);

        // Create aliases.json file
        string aliasesJson = IndexJsonSerializer.Serialize(AppAliases.Empty, IndexSerializationOptions.HumanReadable);
        await File.WriteAllTextAsync(paths.AliasesFile, aliasesJson);
    }
}
