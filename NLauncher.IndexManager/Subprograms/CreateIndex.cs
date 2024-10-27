using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Subprograms;
internal class CreateIndex : Subprogram
{
    public override async ValueTask<SubprogramError?> MainAsync(SubprogramContext ctx)
    {
        static ValidationResult ValidateIndexPath(string path)
        {
            if (!path.StartsWith('/'))
                return ValidationResult.Error("Path must start with a slash.");
            if (!Uri.IsWellFormedUriString(path, UriKind.Relative))
                return ValidationResult.Error("Path is not a valid relative path.");

            return ValidationResult.Success();
        }

        string dirpath = ctx.Paths.Directory;
        if (Directory.Exists(dirpath) && Directory.EnumerateFileSystemEntries(dirpath).Any())
            return new("Cannot create index, directory contains files.");

        AnsiConsole.Write(new Rule("Create Index").HeavyBorder());
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("Repository Info").LeftJustified());
        string owner = AnsiConsole.Prompt(new TextPrompt<string>("Repository Owner:"));
        string repoName = AnsiConsole.Prompt(new TextPrompt<string>("Repository Name:"));

        string indexPath = AnsiConsole.Prompt(new TextPrompt<string>("Path To Index:")
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

        await Create(ctx, meta);

        return null;
    }

    private static async Task Create(SubprogramContext ctx, IndexMeta index)
    {
        // Create directory
        Directory.CreateDirectory(ctx.Paths.Directory);

        // Create index.json file
        string indexJson = JsonSerializer.Serialize(index, IndexJsonSerializerContext.Default.IndexMeta);
        await File.WriteAllTextAsync(ctx.Paths.IndexFile, indexJson);

        // Create url_aliases.json file
        string aliasesJson = JsonSerializer.Serialize(AppAliases.Empty, IndexJsonSerializerContext.Default.AppAliases);
        await File.WriteAllTextAsync(ctx.Paths.AliasesFile, aliasesJson);
    }
}
