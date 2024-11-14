using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.PromptUtils;
internal static class AppManifestPrompt
{
    public const string DefaultTitle = "Choose Application";

    public static async Task<DiscoveredManifest> AskDiscoveredManifest(IndexPaths paths, string title = DefaultTitle)
    {
        ReadOnlyCollection<DiscoveredManifest> manifests = await AnsiConsole.Status().StartAsync(
            "Loading index applications...",
            async _ => await LoadIndexApplications(paths)
        );

        return AnsiConsole.Prompt(
            new SelectionPrompt<DiscoveredManifest>()
                .Title(title)
                .EnableSearch()
                .AddChoices(manifests)
                .UseConverter(static manifest => manifest.Manifest.DisplayName)
        );
    }

    private static async Task<ReadOnlyCollection<DiscoveredManifest>> LoadIndexApplications(IndexPaths paths)
    {
        List<DiscoveredManifest> manifests = new();

        await foreach (DiscoveredManifest manifest in ManifestHelper.DiscoverManifestsAsync(paths))
            manifests.Add(manifest);

        return manifests.AsReadOnly();
    }
}
