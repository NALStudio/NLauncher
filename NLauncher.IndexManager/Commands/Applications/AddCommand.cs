using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Applications;
internal class AddCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        AnsiFormatter.WriteHeader("New Application");

        AnsiFormatter.WriteSectionTitle("App Info");
        string displayName = AnsiConsole.Ask<string>("App Name:");
        string developer = AnsiConsole.Ask<string>("Developer Name:");
        string publisher = AnsiConsole.Ask("Publisher Name:", defaultValue: developer);

        AnsiFormatter.WriteSectionTitle("Release");
        ReleaseState releaseState = AnsiConsole.Prompt(
            new SelectionPrompt<ReleaseState>()
                .Title("Choose Release State")
                .AddChoices(Enum.GetValues<ReleaseState>())
        );

        DateOnly? releaseDate = null;
        if (releaseState.IsReleased()) // Games that haven't yet released can also have a date, but we can assume most of the time this isn't the case so we'll just not bother the user too much.
        {
            DateOnly release = AnsiConsole.Prompt(
                new TextPrompt<DateOnly>("Release Date")
                    .DefaultValue(DateOnly.FromDateTime(DateTime.Now))
            );

            AnsiConsole.WriteLine();
            Spectre.Console.Calendar calendar = new Spectre.Console.Calendar(release.Year, release.Month, release.Day)
                .Culture(CultureInfo.CurrentCulture)
                .AddCalendarEvent(release.Year, release.Month, release.Day);
            AnsiConsole.Write(calendar);

            releaseDate = release;
        }

        AnsiFormatter.WriteSectionTitle("Create Log");
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
        AnsiConsole.WriteLine();
        using (FileChangeTree.ListenAndWrite(settings.Context.Paths.Directory))
        {
            await Create(settings.Context.Paths, manifest);
        }

        return 0;
    }

    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
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
        string manifestJson = IndexJsonSerializer.Serialize(manifest, IndexSerializationOptions.HumanReadable); // write nulls so that user can explicitly see which settings they can change
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
        string publisher = NameToPath(manifest.Publisher);

        string appName = NameToPath(manifest.DisplayName);
        if (index != 0)
            appName += "_" + index.ToString(CultureInfo.InvariantCulture);

        return Path.Join(paths.Directory, publisher, appName);
    }

    private static string NameToPath(string name)
    {
        return AsciiNormalizer.AsLowerCaseAscii(ToSnakeCase(name));
    }

    private static string ToSnakeCase(string sentence)
    {
        return sentence.ToLowerInvariant().Replace(' ', '_');
    }
}
