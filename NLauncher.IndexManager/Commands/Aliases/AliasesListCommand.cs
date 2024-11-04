using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Commands.Aliases;
internal class AliasesListCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public override Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        IndexPaths paths = settings.Context.Paths;

        ReadOnlyDictionary<Guid, AppManifest> manifests = await AnsiConsole.Status().StartAsync(
            "Loading index applications...",
            async _ => await LoadIndexApplications(paths)
        );

        AppAliases aliases = await AnsiConsole.Status().StartAsync(
            "Loading aliases...",
            async _ => await LoadAliases(paths)
        );

        Tree tree = BuildTree(aliases, manifests);
        AnsiConsole.Write(tree);

        return 0;
    }

    private static async Task<ReadOnlyDictionary<Guid, AppManifest>> LoadIndexApplications(IndexPaths paths)
    {
        Dictionary<Guid, AppManifest> manifests = new();

        await foreach (AppManifest manifest in ManifestHelper.DiscoverManifestsAsync(paths))
            manifests.Add(manifest.Uuid, manifest);

        return manifests.AsReadOnly();
    }

    private static async Task<AppAliases> LoadAliases(IndexPaths paths)
    {
        string aliasesJson = await File.ReadAllTextAsync(paths.AliasesFile);
        return IndexJsonSerializer.Deserialize<AppAliases>(aliasesJson) ?? throw new Exception("Could not deserialize app aliases.");
    }

    private static Tree BuildTree(AppAliases aliases, IReadOnlyDictionary<Guid, AppManifest> manifests)
    {
        // Try to get the name, if not possible render the UUID as greyed out.
        Markup TryResolveName(Guid appId)
        {
            if (manifests.TryGetValue(appId, out AppManifest? manifest))
                return new(manifest.DisplayName.EscapeMarkup());
            else
                return new($"[grey]{appId}[/]"); // No need to escape markup, Guid doesn't have [ or ] symbols.
        }

        Tree tree = new("Aliases");
        Dictionary<Guid, TreeNode> nodes = new();

        foreach ((string alias, Guid appId) in aliases.Aliases.OrderBy(static kv => kv.Key))
        {
            Markup name = TryResolveName(appId);

            // If no node for app exists, create one
            if (!nodes.TryGetValue(appId, out TreeNode? node))
            {
                node = tree.AddNode(name);
                nodes.Add(appId, node);
            }

            // Add alias for app
            node.AddNode(alias);
        }

        return tree;
    }

    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }
}
