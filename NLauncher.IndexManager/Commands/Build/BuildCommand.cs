﻿using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using SkiaSharp;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Build;
internal class BuildCommand : AsyncCommand<BuildSettings>
{
    private readonly record struct ImageSize(int Width, int Height);

#pragma warning disable CA1822 // Mark members as static
    public override async Task<int> ExecuteAsync(CommandContext context, BuildSettings settings) => await ExecuteAsync(context, settings, outputPath: settings.OutputPath, minifyOutput: settings.MinifyOutput);
    public async Task<int> ExecuteAsync(CommandContext context, MainSettings settings, string? outputPath, bool minifyOutput)
#pragma warning restore CA1822 // Mark members as static
    {
        // We don't create the directories due to safety reasons (we don't want to accidentally create a bunch of directories in the wrong place)

        string output = outputPath ?? settings.Context.Paths.Directory;
        if (Path.GetExtension(output.AsSpan()).Length < 1) // AsSpan to save on allocating a new string for extension
        {
            string filedate = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            output = Path.Join(output, $"0_build_{filedate}.json");
        }

        string? outputDirectory = Path.GetDirectoryName(output);
        if (!Directory.Exists(outputDirectory))
        {
            Error($"Output directory does not exist: '{outputDirectory}'");
            return 1;
        }

        string? actualOutputPath = await AnsiConsole.Status().StartAsync("Building Index...", ctx => ExecuteWithStatusAsync(ctx, settings, outputPath: output, minifyOutput: minifyOutput));
        if (actualOutputPath is null)
            return 1;

        AnsiConsole.Write(new Markup($"[green]Index built: '{actualOutputPath.EscapeMarkup()}'[/]"));
        return 0;
    }

    private static async Task<string?> ExecuteWithStatusAsync(StatusContext ctx, MainSettings settings, string outputPath, bool minifyOutput)
    {
        IndexManifest? manifest = await TryBuild(ctx, settings.Context.Paths);
        if (manifest is null)
            return null;

        ctx.Status("Serializing output...");
        string json = JsonSerializer.Serialize(manifest, IndexJsonSerializerContext.Default.IndexManifest);

        if (minifyOutput)
        {
            // Cannot remove all whitespace, removes spaces from app names and descriptions
            // Cannot trim each line and remove newlines, is dependant on the input format which isn't a guarantee
            throw new NotImplementedException("JsonSerializerContext doesn't allow for WriteIndented to be changed after the fact.");
        }

        ctx.Status("Writing output...");
        if (File.Exists(outputPath))
        {
            Error($"Output file exists already: '{outputPath}'");
            return null;
        }
        await File.WriteAllTextAsync(outputPath, json); // Encoding defaults to UTF-8

        return outputPath;
    }

    private static async ValueTask<IndexManifest?> TryBuild(StatusContext ctx, IndexPaths paths)
    {
        ctx.Status($"Loading {paths.GetRelativePath(paths.IndexFile)}...");
        IndexMeta? meta = await TryLoadAndDeserialize(paths, paths.IndexFile, IndexJsonSerializerContext.Default.IndexMeta);
        if (meta is null)
            return null;

        ctx.Status($"Loading {paths.GetRelativePath(paths.AliasesFile)}...");
        AppAliases? aliases = await TryLoadAndDeserialize(paths, paths.AliasesFile, IndexJsonSerializerContext.Default.AppAliases);
        if (aliases is null)
            return null;

        ctx.Status($"Loading app manifests...");
        ImmutableArray<IndexEntry>? entries = await GetApps(meta, paths);
        if (!entries.HasValue)
            return null;

        return new IndexManifest()
        {
            Aliases = aliases,
            Metadata = meta,
            Entries = entries.Value
        };
    }

    private static async ValueTask<ImmutableArray<IndexEntry>?> GetApps(IndexMeta meta, IndexPaths paths)
    {
        Dictionary<Guid, IndexEntry> entries = new();

        foreach (DirectoryInfo dir in ManifestHelper.DiscoverManifests(paths))
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
        AppManifest? manifest = await TryLoadAndDeserialize(manifestPaths, manifestPaths.ManifestFile, IndexJsonSerializerContext.Default.AppManifest);
        if (manifest is null)
            return null;

        string? descriptionMarkdown = await TryLoad(manifestPaths, manifestPaths.DescriptionFile);
        if (descriptionMarkdown is null)
            return null;
        string descriptionHtml = DescriptionMarkdownRenderer.RenderDescription(descriptionMarkdown);

        List<IndexAsset> assets = new();
        foreach (string imageFile in manifestPaths.EnumerateImageFiles())
        {
            IndexAsset? asset = TryLoadImage(meta, indexPaths, imageFile);
            if (asset is null)
                AnsiConsole.Write(new Markup("[yellow]Asset ignored.[/]\n"));
            else
                assets.Add(asset);
        }

        return new IndexEntry()
        {
            Manifest = manifest,
            DescriptionHtml = descriptionHtml,
            Assets = new IndexAssetCollection(assets)
        };
    }

    private static IndexAsset? TryLoadImage(IndexMeta meta, IndexPaths paths, string filepath)
    {
        AssetType type = AssetTypeEnum.ParseFilename(filepath);

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
        int expectedWidth = (int)Math.Round(image.Height * (double)aspectRatio);
        if (image.Width != expectedWidth)
        {
            Error($"Invalid image width: {image.Width}. Image must be {expectedWidth} pixels wide (aspect ratio: {aspectRatio}).");
            return null;
        }

        return new(image.Width, image.Height);
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
        AnsiConsole.Write(new Markup($"[red]{msg.EscapeMarkup()}[/]\n"));
    }

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
        if (!baseUrl.EndsWith('/'))
            baseUrl += '/';

        string url = baseUrl + string.Join('/', parts);
        if (!Uri.IsWellFormedUriString(url, UriKind.Relative))
            throw new ArgumentException($"Invalid url: '{url}'");

        return new Uri(url);
    }
}
