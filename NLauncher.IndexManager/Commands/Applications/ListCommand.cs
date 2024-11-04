using NLauncher.Index.Enums;
using NLauncher.Index.Models;
using NLauncher.Index.Models.Applications;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List<string> names = new();

            await foreach (AppManifest manifest in ManifestHelper.DiscoverManifestsAsync(settings.Context.Paths))
            {
                // Find the index where the given manifest should be sorted
                string name = manifest.DisplayName;
                int insertIndex = names.BinarySearch(name);
                if (insertIndex < 0)
                    insertIndex = ~insertIndex;
                names.Insert(insertIndex, name);

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
    private static Text[] FormatValue(AppManifest m) => new Text[]
    {
        GetColorSquare(m),
        new Text(m.DisplayName).Ellipsis(),
        new Text(m.Developer).Ellipsis(),
        new Text(m.Publisher).Ellipsis(),
        new(m.Release.ReleaseDate?.ToString("d") ?? string.Empty),
        new(m.AgeRating.HasValue ? m.AgeRating.Value.ToString("d") : string.Empty),
        new(m.Priority.ToString())
    };

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
