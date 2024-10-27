using NLauncher.Index.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexAsset
{
    public required AssetType Type { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Uri Url { get; init; }
}
