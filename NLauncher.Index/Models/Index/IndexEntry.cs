using NLauncher.Index.Json.Converters;
using NLauncher.Index.Models.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexEntry
{
    public required AppManifest Manifest { get; init; }
    public required string DescriptionHtml { get; init; }
    public required IndexAssetCollection Assets { get; init; }
}
