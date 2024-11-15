using NLauncher.IndexManager.Components.Paths;
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
