using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Index.Models.News;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.Paths;
using SkiaSharp;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Applications.Build;
internal class BuildCommand : AsyncCommand<BuildSettings>, IMainCommand, IMainCommandVariant
{
    private readonly record struct ImageSize(int Width, int Height);

    #region AsyncCommand
    public override async Task<int> ExecuteAsync(CommandContext context, BuildSettings settings) => await ExecuteAsync(settings, outputPath: settings.OutputPath, humanReadable: !settings.MinifyOutput);
    public override ValidationResult Validate(CommandContext context, BuildSettings settings) => Validate(settings);
    #endregion

    #region IMainCommand
    public async Task<int> ExecuteAsync(MainSettings settings) => await ExecuteAsync(settings, outputPath: null, humanReadable: false);
    public ValidationResult Validate(MainSettings settings) => ValidationResult.Success();
    #endregion

    #region IMainCommandVariant
    public async Task<int> ExecuteVariantAsync(MainSettings settings) => await ExecuteAsync(settings, outputPath: null, humanReadable: true);
    #endregion

    public static async Task<int> ExecuteAsync(MainSettings settings, string? outputPath, bool humanReadable)
    {
        // We don't create the directories due to safety reasons (we don't want to accidentally create a bunch of directories in the wrong place)

        string? actualOutputPath = await AnsiConsole.Status().StartAsync("Building Index...", ctx => ExecuteWithStatusAsync(ctx, settings, outputPath: outputPath, humanReadable: humanReadable));
        if (actualOutputPath is null)
            return 1;

        AnsiConsole.Write("Index built: ");
        AnsiConsole.Write(AnsiFormatter.CreatedFile(actualOutputPath));
        AnsiConsole.WriteLine();

        return 0;
    }

    private static async Task<string?> ExecuteWithStatusAsync(StatusContext ctx, MainSettings settings, string? outputPath, bool humanReadable)
    {
        IndexPaths paths = settings.Context.Paths;

        ctx.Status($"Loading {paths.GetRelativePath(paths.IndexFile)}...");
        IndexMeta? meta = await TryLoadAndDeserialize(paths, paths.IndexFile, IndexJsonContext.Default.IndexMeta);
        if (meta is null)
            return null;

        IndexManifest? manifest = await TryBuild(ctx, meta, paths);
        if (manifest is null)
            return null;

        ctx.Status("Serializing output...");
        IndexJsonContext jsonContext = humanReadable ? IndexJsonContext.HumanReadable : IndexJsonContext.Default;
        string json = JsonSerializer.Serialize(manifest, jsonContext.IndexManifest);

        ctx.Status("Writing output...");
        outputPath ??= Path.Join(paths.Directory, meta.IndexManifestPath);
        outputPath = Path.GetFullPath(outputPath);

        string? outputDirectory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDirectory))
        {
            Error($"Output directory does not exist: '{outputDirectory}'");
            return null;
        }
        await File.WriteAllTextAsync(outputPath, json); // Encoding defaults to UTF-8

        return outputPath;
    }

    private static async ValueTask<IndexManifest?> TryBuild(StatusContext ctx, IndexMeta meta, IndexPaths paths)
    {
        ctx.Status($"Loading {paths.GetRelativePath(paths.AliasesFile)}...");
        AppAliases? aliases = await TryLoadAndDeserialize(paths, paths.AliasesFile, IndexJsonContext.Default.AppAliases);
        if (aliases is null)
            return null;

        ctx.Status("Loading news...");
        ImmutableArray<NewsEntry>? news = await GetNews(meta, paths);
        if (!news.HasValue)
            return null;

        ctx.Status($"Loading app manifests...");
        ImmutableArray<IndexEntry>? entries = await GetApps(meta, paths);
        if (!entries.HasValue)
            return null;

        return new IndexManifest()
        {
            Aliases = aliases,
            Metadata = meta,
            News = news.Value,
            Entries = entries.Value
        };
    }

    private static async ValueTask<ImmutableArray<NewsEntry>?> GetNews(IndexMeta meta, IndexPaths paths)
    {
        List<NewsEntry> entries = new();

        foreach ((int index, NewsPaths nPaths) in NewsSlots.EnumeratePathIndexes(paths, existingOnly: true))
        {
            Debug.Assert(nPaths.Exists());
            NewsEntry? news = await TryConstructNews(meta, paths, index, nPaths);
            if (news is null)
                return null;

            entries.Add(news);
        }

        return entries.ToImmutableArray();
    }

    private static async ValueTask<NewsEntry?> TryConstructNews(IndexMeta meta, IndexPaths indexPaths, int index, NewsPaths newsPaths)
    {
        if (!File.Exists(newsPaths.LogoImageFile))
        {
            NotFound(newsPaths, newsPaths.LogoImageFile);
            return null;
        }
        Uri logoImageFile = ConstructGitHubAssetPath(meta, indexPaths, newsPaths.LogoImageFile);

        if (!File.Exists(newsPaths.BackgroundImageFile))
        {
            NotFound(newsPaths, newsPaths.BackgroundImageFile);
            return null;
        }
        Uri backgroundImageFile = ConstructGitHubAssetPath(meta, indexPaths, newsPaths.BackgroundImageFile);
        ImageSize? backgroundSize = ValidateImageSize(newsPaths, newsPaths.BackgroundImageFile, new AssetTypeEnum.Aspect(16, 9));
        if (!backgroundSize.HasValue)
            return null;
        double? backgroundBrightness = ComputeImageBrightness(newsPaths, newsPaths.BackgroundImageFile);
        if (!backgroundBrightness.HasValue)
            return null;

        NewsManifest? manifest = await TryLoadAndDeserialize(newsPaths, newsPaths.NewsFile, IndexJsonContext.Default.NewsManifest);
        if (manifest is null)
            return null;

        return new NewsEntry()
        {
            Index = index,
            Manifest = manifest,
            Assets = new NewsEntryAssets()
            {
                Background = backgroundImageFile,
                BackgroundBrightness = backgroundBrightness.Value,
                Logo = logoImageFile
            }
        };
    }

    private static async ValueTask<ImmutableArray<IndexEntry>?> GetApps(IndexMeta meta, IndexPaths paths)
    {
        Dictionary<Guid, IndexEntry> entries = new();

        foreach (DirectoryInfo dir in ManifestDiscovery.DiscoverManifestDirectories(paths))
        {
            ManifestPaths mPaths = new(dir.FullName);

            IndexEntry? e = await TryConstructEntry(meta, paths, mPaths);
            if (e is null)
                return null;

            bool added = entries.TryAdd(e.Manifest.Uuid, e);
            if (!added)
            {
                IndexEntry existingEntry = entries[e.Manifest.Uuid];

                Error($"Duplicate application ID's detected: '{existingEntry.Manifest.DisplayName}' and '{e.Manifest.DisplayName}'");
                return null;
            }
        }

        return entries.Values.ToImmutableArray();
    }

    private static async ValueTask<IndexEntry?> TryConstructEntry(IndexMeta meta, IndexPaths indexPaths, ManifestPaths manifestPaths)
    {
        AppManifest? manifest = await TryLoadAndDeserialize(manifestPaths, manifestPaths.ManifestFile, IndexJsonContext.Default.AppManifest);
        if (manifest is null)
            return null;

        string? descriptionMarkdown = await TryLoad(manifestPaths, manifestPaths.DescriptionFile);
        if (descriptionMarkdown is null)
            return null;

        List<IndexAsset> assets = new();
        foreach (string imageFile in manifestPaths.EnumerateImageFiles())
        {
            IndexAsset? asset = TryLoadImage(meta, indexPaths, imageFile);
            if (asset is null)
            {
                string assetRelativePath = Path.GetRelativePath(indexPaths.Directory, imageFile);
                AnsiConsole.MarkupLine($"[yellow]Asset ignored: '{assetRelativePath.EscapeMarkup()}'[/]");
            }
            else
            {
                assets.Add(asset);
            }
        }

        return new IndexEntry()
        {
            Manifest = manifest,
            DescriptionMarkdown = descriptionMarkdown,
            Assets = new IndexAssetCollection(assets)
        };
    }

    private static IndexAsset? TryLoadImage(IndexMeta meta, IndexPaths paths, string filepath)
    {
        if (!AssetTypeEnum.TryParseFilename(filepath, out AssetType type))
        {
            Error($"File name: '{Path.GetFileName(filepath)}' could not be parsed into a type.");
            return null;
        }

        ImageSize? size = ValidateImageSize(paths, filepath, type.AspectRatio());
        if (!size.HasValue)
            return null;

        Uri url = ConstructGitHubAssetPath(meta, paths, filepath);
        return new()
        {
            Type = type,
            Width = size.Value.Width,
            Height = size.Value.Height,
            Url = url
        };
    }

    private static ImageSize? ValidateImageSize(DirectoryPathProvider paths, string filepath, AssetTypeEnum.Aspect aspectRatio)
    {
        if (!File.Exists(filepath))
        {
            NotFound(paths, filepath);
            return null;
        }

        SKImage? image = SKImage.FromEncodedData(filepath);
        int expectedWidth = (int)Math.Round(image.Height * (decimal)aspectRatio, MidpointRounding.AwayFromZero);
        if (image.Width != expectedWidth)
        {
            Error($"Invalid image width: {image.Width}. Image must be {expectedWidth} pixels wide (aspect ratio: {aspectRatio}). ({filepath})");
            return null;
        }

        return new(image.Width, image.Height);
    }

    private static double? ComputeImageBrightness(DirectoryPathProvider paths, string filepath)
    {
        if (!File.Exists(filepath))
        {
            NotFound(paths, filepath);
            return null;
        }

        SKBitmap bitmap = SKBitmap.Decode(filepath);

        decimal brightnessSum = 0m;
        SKColor[] pixels = bitmap.Pixels;
        foreach (SKColor c in pixels)
            brightnessSum += (decimal)GetBrightness(c);

        double brightness = (double)(brightnessSum / pixels.Length);
        Debug.Assert(brightness >= 0 && brightness <= 1);
        return brightness;
    }

    private static async ValueTask<string?> TryLoad(DirectoryPathProvider paths, string filepath)
    {
        if (!File.Exists(filepath))
        {
            NotFound(paths, filepath);
            return null;
        }

        return await File.ReadAllTextAsync(filepath);
    }

    private static async ValueTask<T?> TryLoadAndDeserialize<T>(DirectoryPathProvider paths, string filepath, JsonTypeInfo<T> jsonTypeInfo) where T : class
    {
        string? json = await TryLoad(paths, filepath);
        if (json is null)
            return null;

        T? deserialized = JsonSerializer.Deserialize(json, jsonTypeInfo);
        if (deserialized is null)
        {
            Error($"{paths.GetRelativePath(filepath)} could not be deserialized.");
            return null;
        }

        return deserialized;
    }

    private static void NotFound(DirectoryPathProvider paths, string filepath)
    {
        string relPath = paths.GetRelativePath(filepath);
        Error($"No {relPath} found in directory: '{paths.Directory}'");
    }

    private static void Error(string msg)
    {
        AnsiConsole.MarkupLine($"[red]{msg.EscapeMarkup()}[/]");
    }

    // use IndexPaths so that the relative path is computed correctly.
    private static Uri ConstructGitHubAssetPath(IndexMeta meta, IndexPaths paths, string assetPath)
    {
        string assetRelativePath = paths.GetRelativePath(assetPath);
        string repoRelativePath = Path.Join(meta.Repository.Path, assetRelativePath);

        string assetUrl = repoRelativePath.Replace('\\', '/');

        return ConstructUrl(
            "https://raw.githubusercontent.com/",
            meta.Repository.Owner, meta.Repository.Repo, "refs", "heads", meta.Repository.Branch, assetUrl
        );
    }

    private static Uri ConstructUrl(string baseUrl, params string[] parts)
    {
        // Trim leading and trailing slashes from all parts
        for (int i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim('/');

        string partsUrl = string.Join('/', parts);
        if (!Uri.IsWellFormedUriString(partsUrl, UriKind.Relative))
            throw new ArgumentException($"Invalid URL path: '{partsUrl}'");

        if (!baseUrl.EndsWith('/'))
            baseUrl += '/';
        string url = baseUrl + partsUrl;
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            throw new ArgumentException($"Invalid URL: '{url}'");

        return new Uri(url);
    }

    // https://www.alienryderflex.com/hsp.html
    private static double GetBrightness(SKColor color)
    {
        double r = color.Red / 255d;
        double g = color.Green / 255d;
        double b = color.Blue / 255d;

        return Math.Sqrt(
            (r * r * 0.299d)
            + (g * g * 0.587d)
            + (b * b * 0.114d)
        );
    }
}
