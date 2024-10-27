using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexRepository
{
    public required string Owner { get; init; }
    public required string Repo { get; init; }
    public string Path { get; init; } = string.Empty;

    public string? ReleaseAssetName { get; init; }
}
