using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.News;
using NLauncher.Index.Models.News.Interactivity;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using NLauncher.IndexManager.Components.Paths;
using NLauncher.IndexManager.Components.PromptUtils;
using NLauncher.IndexManager.Models;
using SkiaSharp;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.News;
internal class NewsAddCommand : AsyncCommand<MainSettings>, IMainCommand
{
    private static readonly ImmutableArray<string> DefaultTitles =
    [
        "Now Available",
        "New Update"
    ];

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
    public ValidationResult Validate(MainSettings settings) => ValidationResult.Success();

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        IndexPaths indexPaths = settings.Context.Paths;
        NewsPaths? paths = await NewsSlotPrompt.AskNewsPath(indexPaths, allowReserved: false);
        if (paths is null)
        {
            NewsSlotPrompt.PrintNoSlotsAvailable();
            return 1;
        }

        string title = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices(DefaultTitles)
                .AddChoices(string.Empty)
                .UseConverter(static s => string.IsNullOrEmpty(s) ? "<custom>" : s)
        );
        if (string.IsNullOrEmpty(title))
            title = AnsiConsole.Ask<string>("Custom title:");

        string inter = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("explore", "play_now")
        );

        NewsInteractivity interactivity;
        if (inter == "explore")
        {
            interactivity = new ExploreNewsInteractivity();
        }
        else if (inter == "play_now")
        {
            DiscoveredManifest app = await AppManifestPrompt.AskDiscoveredManifest(indexPaths);
            interactivity = new PlayNowNewsInteractivity()
            {
                AppId = app.Manifest.Uuid
            };
        }
        else
        {
            throw new NotImplementedException($"Interactivity not implemented: '{inter}'");
        }

        NewsManifest manifest = new()
        {
            Title = title,
            Text = "EXAMPLE: This is an example news text. You can write a small paragraph here on the things you want to say about the given news.",
            Interactivity = interactivity,
        };

        using (FileChangeTree.ListenAndWrite(indexPaths.Directory))
        {
            await AnsiConsole.Status().StartAsync(
                "Creating news...",
                async _ =>
                {
                    Task task = new(() => Execute(paths, manifest), TaskCreationOptions.LongRunning);
                    task.Start();
                    await task;
                }
            );
        }

        return 0;
    }

    private static void Execute(NewsPaths paths, NewsManifest manifest)
    {
        Debug.Assert(!Directory.Exists(paths.Directory));
        Directory.CreateDirectory(paths.Directory);

        // Write json
        string manifestJson = IndexJsonSerializer.Serialize(manifest, IndexSerializationOptions.HumanReadable);
        File.WriteAllText(paths.NewsFile, manifestJson); // use synchronous method since SkiaSharp is also synchronous

        // Write images
        WriteTmpImage(paths.LogoImageFile, (2048, 512));
        WriteTmpImage(paths.BackgroundImageFile, (1920, 1080));
    }

    private static void WriteTmpImage(string path, (int W, int H) size)
    {
        SKBitmap bitmap = new(size.W, size.H);
        bitmap.Erase(new SKColor(255, 0, 0));

        using FileStream fs = File.OpenWrite(path);
        bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
    }
}
