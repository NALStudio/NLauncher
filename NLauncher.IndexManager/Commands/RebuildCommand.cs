using NLauncher.Index.Interfaces;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.News;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using NLauncher.IndexManager.Components.Paths;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;

namespace NLauncher.IndexManager.Commands.Applications;
internal class RebuildCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        IndexPaths paths = settings.Context.Paths;

        bool sure = AnsiConsole.Confirm("Are you sure you want to rebuild the entire index?");
        if (!sure)
        {
            AnsiFormatter.WriteOperationCancelled();
            return 0;
        }

        using (FileChangeTree.ListenAndWrite(paths.Directory))
        {
            await AnsiConsole.Status().StartAsync(
                "Rebuilding index...",
                async ctx => await Task.Run(() => Rebuild(ctx, paths))
            );
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Index rebuilt.[/]");
        return 0;
    }

    private static async Task Rebuild(StatusContext ctx, IndexPaths paths)
    {
        ctx.Status("Rebuilding news...");
        foreach (NewsPaths newsPaths in NewsSlots.EnumeratePaths(paths, existingOnly: true))
        {
            Debug.Assert(newsPaths.Exists());
            await RebuildManifest<NewsManifest>(newsPaths.NewsFile);
        }

        ctx.Status("Rebuilding apps...");
        foreach (FileInfo f in ManifestDiscovery.DiscoverManifestFiles(paths))
            await RebuildManifest<AppManifest>(f.FullName);
    }

    private static async Task RebuildManifest<T>(string filepath) where T : IIndexSerializable
    {
        string oldJson = await File.ReadAllTextAsync(filepath);
        T manifest = IndexJsonSerializer.Deserialize<T>(oldJson)
            ?? throw new InvalidOperationException($"Could not deserialize file: '{filepath}'");

        string newJson = IndexJsonSerializer.Serialize(manifest, IndexSerializationOptions.HumanReadable);
        await File.WriteAllTextAsync(filepath, newJson);
    }

    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
}
