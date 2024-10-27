using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Subprograms;

public class SubprogramError
{
    public string Message { get; }

    public SubprogramError(string message)
    {
        Message = message;
    }
}

internal class SubprogramContext
{
    public IndexPaths Paths { get; }

    public SubprogramContext(string indexDirectoryPath)
    {
        Paths = new IndexPaths(indexDirectoryPath);
    }

    public bool IndexExists => Directory.Exists(Paths.Directory) && File.Exists(Paths.IndexFile);
    public void ThrowIfNotExists()
    {
        if (!IndexExists)
            throw new InvalidOperationException($"Index not found at path: '{Paths.Directory}'");
    }
}

internal class IndexPaths
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

internal abstract class Subprogram
{
    public abstract ValueTask<SubprogramError?> MainAsync(SubprogramContext ctx);
}
