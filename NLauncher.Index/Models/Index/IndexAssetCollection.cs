using NLauncher.Index.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexAssetCollection
{
    public ImmutableArray<IndexAsset> Assets { get; }

    private readonly ILookup<AssetType, IndexAsset> typeLookup;

    public IndexAssetCollection(ImmutableArray<IndexAsset> assets)
    {
        Assets = assets;
        typeLookup = assets.ToLookup(key => key.Type);
    }

    public IndexAsset? Largest(AssetType type) => typeLookup[type].MaxBy(static asset => asset.Height);
    public IndexAsset? LargestWidth(AssetType type) => typeLookup[type].MaxBy(static asset => asset.Width);

    public IndexAsset? Smallest(AssetType type) => typeLookup[type].MinBy(static asset => asset.Height);
    public IndexAsset? SmallestWidth(AssetType type) => typeLookup[type].MinBy(static asset => asset.Width);

    public IndexAsset? Closest(AssetType type, int height) => typeLookup[type].MinBy(asset => Math.Abs(height - asset.Height));
    public IndexAsset? ClosestWidth(AssetType type, int width) => typeLookup[type].MinBy(asset => Math.Abs(width - asset.Width));
}
