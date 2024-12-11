using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.News;
public class NewsEntry
{
    /// <summary>
    /// Index might not be sequencial and it should only be used to sort the entries into the correct order.
    /// </summary>
    public required int Index { get; init; }

    public required NewsManifest Manifest { get; init; }
    public required NewsEntryAssets Assets { get; init; }
}
