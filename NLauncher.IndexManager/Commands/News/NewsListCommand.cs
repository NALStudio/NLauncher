using NLauncher.Index.Json;
using NLauncher.Index.Models.News;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.Paths;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.News;
internal class NewsListCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
    public ValidationResult Validate(MainSettings settings) => ValidationResult.Success();

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        Table t = new Table().ShowRowSeparators();
        t.AddColumns("Logo", "Title", "Text");
        t.Columns[1].Centered();

        await AnsiConsole.Live(t).StartAsync(async ctx =>
        {
            foreach (NewsPaths paths in NewsSlots.EnumeratePaths(settings.Context.Paths, existingOnly: true))
            {
                List<IRenderable> renderables = new();
                await foreach (IRenderable r in CreateRenderables(paths))
                    renderables.Add(r);

                t.AddRow(renderables);
                ctx.Refresh();
            }
        });

        return 0;
    }

    private static async IAsyncEnumerable<IRenderable> CreateRenderables(NewsPaths paths)
    {
        string newsJson = await File.ReadAllTextAsync(paths.NewsFile);
        NewsManifest news = JsonSerializer.Deserialize(newsJson, IndexJsonContext.Default.NewsManifest) ?? throw new Exception("Could not deserialize.");

        yield return new CanvasImage(paths.BackgroundImageFile);
        yield return new Text(news.Title);
        yield return new Text(news.Text);
    }
}
