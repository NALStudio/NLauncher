using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexMeta
{
    [JsonPropertyName("indexmanifest_path")]
    public required string IndexManifestPath { get; init; }

    public required IndexRepository Repository { get; init; }
}
