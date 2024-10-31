using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands;
internal class AddCommand : AsyncCommand<MainSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings)
    {
        AnsiConsole.Write(new Rule("New Application").HeavyBorder());
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("App Info").LeftJustified());
        string displayName = AnsiConsole.Ask<string>("App Name:");
        string developer = AnsiConsole.Ask<string>("Developer Name:");
        string publisher = AnsiConsole.Ask("Publisher Name:", defaultValue: developer);
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("Release").LeftJustified());
        ReleaseState releaseState = AnsiConsole.Prompt(
            new SelectionPrompt<ReleaseState>()
                .Title("Choose Release State")
                .AddChoices(Enum.GetValues<ReleaseState>())
        );

        DateOnly? releaseDate = null;
        if (releaseState.IsReleased()) // Games that haven't yet released can also have a date, but we can assume most of the time this isn't the case so we'll just not bother the user too much.
        {
            releaseDate = AnsiConsole.Prompt(
                new TextPrompt<DateOnly>("Release Date")
                    .DefaultValue(DateOnly.FromDateTime(DateTime.Now))
            );
        }

        AppManifest manifest = new()
        {
            DisplayName = displayName,
            Uuid = Guid.NewGuid(),
            Developer = developer,
            Publisher = publisher,
            Release = new AppRelease()
            {
                State = releaseState,
                ReleaseDate = releaseDate
            },

            Color = null,
            Versions = ImmutableArray<AppVersion>.Empty
        };

        AnsiConsole.WriteLine("Adding Application...");
        await Create(settings.Context.Paths, manifest);
        AnsiConsole.Write(new Markup("[green]Application added.[/]\n"));

        return 0;
    }

    private static async Task Create(IndexPaths indexPaths, AppManifest manifest)
    {
        string dirpath;
        int dirpathIndex = 0;
        do
        {
            dirpath = GetDirPath(indexPaths, manifest, dirpathIndex);
            dirpathIndex++;
        }
        while (Path.Exists(dirpath));

        ManifestPaths paths = new(dirpath);

        // Create directory
        Directory.CreateDirectory(paths.Directory);

        // Create manifest.json
        string manifestJson = JsonSerializer.Serialize(manifest, IndexJsonSerializerContext.Default.AppManifest);
        await File.WriteAllTextAsync(paths.ManifestFile, manifestJson);

        // Create description.md
        string descriptionMd = $"""
            # {manifest.DisplayName.EscapeMarkup()}'s Description

            This is the description of {manifest.Developer.EscapeMarkup()}'s app '{manifest.DisplayName.EscapeMarkup()}'.
            """;
        await File.WriteAllTextAsync(paths.DescriptionFile, descriptionMd);
    }

    private static string GetDirPath(IndexPaths paths, AppManifest manifest, int index)
    {
        string publisher = ToSnakeCase(manifest.Publisher);

        string appName = ToSnakeCase(manifest.DisplayName);
        if (index != 0)
            appName += "_" + index.ToString(CultureInfo.InvariantCulture);

        return Path.Join(paths.Directory, publisher, appName);
    }

    private static string ToSnakeCase(string sentence)
    {
        return sentence.ToLowerInvariant().Replace(' ', '_');
    }
}
