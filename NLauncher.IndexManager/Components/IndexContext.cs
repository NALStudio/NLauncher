using NLauncher.IndexManager.Commands.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components;

internal sealed class IndexContext
{
    public IndexPaths Paths { get; }
    private IndexContext(IndexPaths paths)
    {
        Paths = paths;
    }

    public bool IndexExists => Directory.Exists(Paths.Directory) && File.Exists(Paths.IndexFile);
    public void ThrowIfIndexNotExists()
    {
        if (!IndexExists)
            throw new InvalidOperationException($"Index not found at path: '{Paths.Directory}'");
    }

    public static IndexContext? TryResolve(string path)
    {
        string? filename = Path.GetFileName(path);
        string? dirpath;
        if (string.Equals(filename, Constants.IndexMetaFilename, StringComparison.OrdinalIgnoreCase))
        {
            // Path points to a file
            dirpath = Path.GetDirectoryName(path);
        }
        else
        {
            // Path points to a dictionary
            dirpath = path;
        }

        if (dirpath is not null)
        {
            return new IndexContext(
                new IndexPaths(dirpath)
            );
        }
        else
        {
            return null;
        }
    }
}

internal sealed class IndexPaths
{
    /// <summary>
    /// This directory is guaranteed to exist if the index exists.
    /// </summary>
    public string Directory { get; }

    public IndexPaths(string directory)
    {
        Directory = directory;

        IndexFile = Path.Join(directory, "index.json");
        AliasesFile = Path.Join(directory, "aliases.json");
    }

    /// <summary>
    /// This file is guaranteed to exist if the index exists.
    /// </summary>
    public string IndexFile { get; }

    public string AliasesFile { get; }
}

internal sealed class ManifestPaths
{
    public string Directory { get; }
    public string AssetsDirectory { get; }

    public ManifestPaths(string directory)
    {
        Directory = directory;
        AssetsDirectory = Path.Join(directory, "assets");

        ManifestFile = Path.Join(directory, "manifest.json");
        DescriptionFile = Path.Join(directory, "description.md");

        IconImageFile = Path.Join(directory, "icon.png");
        BannerImageFile = Path.Join(directory, "banner.png");
        PanelImageFile = Path.Join(directory, "panel.png");
    }

    public string ManifestFile { get; }
    public string DescriptionFile { get; }

    public string IconImageFile { get; }
    public string BannerImageFile { get; }
    public string PanelImageFile { get; }
}