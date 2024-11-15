using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.Paths;
internal sealed class IndexPaths : DirectoryPathProvider
{
    public string NewsDirectory { get; }

    public IndexPaths(string directory) : base(directory)
    {
        NewsDirectory = Path.Join(directory, "@news");

        IndexFile = Path.Join(directory, "index.json");
        AliasesFile = Path.Join(directory, "aliases.json");
    }

    public string IndexFile { get; }
    public string AliasesFile { get; }
}
