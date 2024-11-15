using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.Paths;
internal abstract class DirectoryPathProvider
{
    public string Directory { get; }

    protected DirectoryPathProvider(string directory)
    {
        Directory = directory;
    }

    public string GetRelativePath(string path)
    {
        return Path.GetRelativePath(Directory, path);
    }
}