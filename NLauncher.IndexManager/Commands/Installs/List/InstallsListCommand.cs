﻿using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.Paths;
using NLauncher.IndexManager.Components.PromptUtils;
using NLauncher.IndexManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NLauncher.IndexManager.Commands.Installs.List;
internal class InstallsListCommand : AsyncCommand<InstallsListSettings>, IMainCommand, IMainCommandVariant
{
    public override async Task<int> ExecuteAsync(CommandContext context, InstallsListSettings settings) => await Execute(settings, appFilter: settings.AppId, showAllVersions: settings.ShowAllVersions);
    public override ValidationResult Validate(CommandContext context, InstallsListSettings settings) => Validate(settings);
    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        DiscoveredManifest app = await AppManifestPrompt.AskDiscoveredManifest(settings.Context.Paths);
        return await Execute(settings, appFilter: app.Manifest.Uuid, showAllVersions: true);
    }
    public async Task<int> ExecuteVariantAsync(MainSettings settings)
    {
        return await Execute(settings, appFilter: null, showAllVersions: false);
    }

    private static async Task<int> Execute(MainSettings settings, Guid? appFilter, bool showAllVersions)
    {
        IndexPaths paths = settings.Context.Paths;

        List<AppManifest> apps = new();
        await foreach (DiscoveredManifest disc in ManifestDiscovery.DiscoverManifestsAsync(paths))
        {
            bool appNotValid = appFilter.HasValue && disc.Manifest.Uuid != appFilter.Value;
            if (!appNotValid)
                apps.Add(disc.Manifest);
        }

        Tree tree = new("Installs");
        foreach (AppManifest app in apps)
        {
            TreeNode node = tree.AddNode(app.DisplayName.EscapeMarkup());

            if (showAllVersions)
            {
                foreach (AppVersion version in app.Versions)
                {
                    TreeNode versionNode = node.AddNode($"{version.VerNum} ({version.Identifier})");
                    AddInstalls(versionNode, version);
                }
            }
            else
            {
                AppVersion? latest = app.GetLatestVersion();
                if (latest is not null)
                    AddInstalls(node, latest);
            }
        }

        AnsiConsole.Write(tree);

        return 0;
    }

    private static void AddInstalls(TreeNode node, AppVersion version)
    {
        for (int i = 0; i < version.Installs.Length; i++)
        {
            AppInstall install = version.Installs[i];
            node.AddNode(RenderInstall(i, install));
        }
    }

    private static Table RenderInstall(int index, AppInstall install)
    {
        Table t = new Table().AddColumns("", "").HideHeaders();

        t.AddRow("Priority", FormatPriority(index + 1));
        t.AddRow("Type", install.GetType().Name.EscapeMarkup());
        t.AddRow("Platforms", install.GetSupportedPlatforms().ToString().EscapeMarkup());

        return t;
    }

    private static string FormatPriority(int priority)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(priority, 0);

        int priorityLastDigit = priority % 10;
        string suffix = priorityLastDigit switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };

        return priority.ToString() + suffix;
    }
}
