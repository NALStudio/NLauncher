using NLauncher.Index.Enums;
using NLauncher.Index.Helpers;
using NLauncher.Index.Models;
using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace NLauncher.IndexManager.Commands.Applications;
internal class ListCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        Table table = new Table()
            .Title("Applications")
            .AddColumns(ColumnNames())
            .ShowRowSeparators();

        table.Columns[0].Padding = new Padding(0);

        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            // List to keep track of manifest sort order
            // this list is binary searched to find the index where to sort new values
            List<AppManifest> manifests = new();

            await foreach (DiscoveredManifest discoveredManifest in ManifestDiscovery.DiscoverManifestsAsync(settings.Context.Paths))
            {
                AppManifest manifest = discoveredManifest.Manifest;

                // Find the index where the given manifest should be sorted
                int insertIndex = manifests.BinarySearch(manifest, AppManifestSorter.Comparer.Instance);
                if (insertIndex < 0)
                    insertIndex = ~insertIndex;
                manifests.Insert(insertIndex, manifest);

                IRenderable[] row = FormatValue(manifest);

                // Insert value to index, table SHOULD BE in sync with the names list
                table.InsertRow(insertIndex, row);

                ctx.Refresh();
            }
        });

        return 0;
    }

    private static TableColumn[] ColumnNames() => new TableColumn[]
    {
        new(""), // Color
        new("Name"),
        new("Developer"),
        new("Publisher"),
        new TableColumn("Release Date").Centered(),
        new TableColumn("Age Rating").Centered(),
        new TableColumn("Priority").Centered()
    };
    private static Text[] FormatValue(AppManifest m)
    {
        Style style = new(foreground: GetEnvironmentColor(m.Environment));

        return new Text[]
        {
            GetColorSquare(m),
            new Text(m.DisplayName, style).Ellipsis(),
            new Text(m.Developer, style).Ellipsis(),
            new Text(m.Publisher, style).Ellipsis(),
            new(m.Release.ReleaseDate?.ToString("d") ?? string.Empty, style),
            new(m.AgeRating.HasValue ? m.AgeRating.Value.ToString("d") : string.Empty, style),
            new(m.Priority.ToString(), style)
        };
    }

    private static ConsoleColor? GetEnvironmentColor(IndexEnvironment? env)
    {
        return env switch
        {
            IndexEnvironment.Release => ConsoleColor.Cyan,
            IndexEnvironment.Development => ConsoleColor.Yellow,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(env)),
        };
    }

    private static Text GetColorSquare(AppManifest m)
    {
        Color? color;
        if (m.Color is Color24 c)
            color = new(c.R, c.G, c.B);
        else
            color = null;

        return new Text(" ", new Style(background: color));
    }

    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
}
