using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NLauncher.IndexManager.Commands.Applications;
internal class RebuildCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        bool sure = AnsiConsole.Confirm("Are you sure you want to rebuild the entire index?");
        if (!sure)
        {
            AnsiFormatter.WriteOperationCancelled();
            return 0;
        }

        await AnsiConsole.Status().StartAsync(
            "Rebuilding index...",
            async _ => await Task.Run(() => Rebuild(settings.Context.Paths))
        );

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Index rebuilt.[/]");
        return 0;
    }

    private static async Task Rebuild(IndexPaths paths)
    {
        foreach (FileInfo f in ManifestHelper.DiscoverManifestFiles(paths))
            await RebuildManifestFile(f);
    }

    private static async Task RebuildManifestFile(FileInfo file)
    {
        string oldJson = await File.ReadAllTextAsync(file.FullName);
        AppManifest manifest = IndexJsonSerializer.Deserialize<AppManifest>(oldJson)
            ?? throw new InvalidOperationException($"Could not deserialize file: '{file.FullName}'");

        string newJson = IndexJsonSerializer.Serialize(manifest, IndexSerializationOptions.HumanReadable);
        await File.WriteAllTextAsync(file.FullName, newJson);
    }

    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
}
