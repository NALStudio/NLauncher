using NLauncher.Index.Enums;
using NLauncher.Index.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;

[JsonConverter(typeof(JsonIndexAssetCollectionConverter))]
public class IndexAssetCollection
{
    public ImmutableArray<IndexAsset> Assets { get; }

    private readonly ImmutableDictionary<AssetType, ImmutableArray<IndexAsset>> typeLookup;

    public IndexAssetCollection(IEnumerable<IndexAsset> assets) : this(assets.ToImmutableArray()) { }
    public IndexAssetCollection(ImmutableArray<IndexAsset> assets)
    {
        Assets = assets;
        typeLookup = assets.GroupBy(key => key.Type).ToImmutableDictionary(key => key.Key, value => value.ToImmutableArray());
    }

    public IndexAsset? Largest(AssetType type) => MaxBy(static asset => asset.Height, type);
    public IndexAsset? Largest(params IEnumerable<AssetType> types) => MaxBy(static asset => asset.Height, types);

    public IndexAsset? LargestWidth(AssetType type) => MaxBy(static asset => asset.Width, type);
    public IndexAsset? LargestWidth(params IEnumerable<AssetType> types) => MaxBy(static asset => asset.Width, types);

    public IndexAsset? Smallest(AssetType type) => MinBy(static asset => asset.Height, type);
    public IndexAsset? Smallest(params IEnumerable<AssetType> types) => MinBy(static asset => asset.Height, types);

    public IndexAsset? SmallestWidth(AssetType type) => MinBy(static asset => asset.Width, type);
    public IndexAsset? SmallestWidth(params IEnumerable<AssetType> types) => MinBy(static asset => asset.Width, types);

    public IndexAsset? Closest(int height, AssetType type) => MinBy(asset => Math.Abs(height - asset.Height), type);
    public IndexAsset? Closest(int height, params IEnumerable<AssetType> types) => MinBy(asset => Math.Abs(height - asset.Height), types);

    public IndexAsset? ClosestWidth(int width, AssetType type) => MinBy(asset => Math.Abs(width - asset.Width), type);
    public IndexAsset? ClosestWidth(int width, params IEnumerable<AssetType> types) => MinBy(asset => Math.Abs(width - asset.Width), types);

    private ImmutableArray<IndexAsset> GetAssets(AssetType type)
    {
        if (typeLookup.TryGetValue(type, out ImmutableArray<IndexAsset> value))
            return value;
        return ImmutableArray<IndexAsset>.Empty;
    }

    private IndexAsset? MaxBy(Func<IndexAsset, int> keySelector, AssetType type)
    {
        ImmutableArray<IndexAsset> assets = GetAssets(type);
        return assets.Length > 0 ? assets.MaxBy(keySelector) : null;
    }
    private IndexAsset? MaxBy(Func<IndexAsset, int> keySelector, IEnumerable<AssetType> types)
    {
        foreach (AssetType type in types)
        {
            IndexAsset? asset = MaxBy(keySelector, type);
            if (asset is not null)
                return asset;
        }

        return null;
    }

    private IndexAsset? MinBy(Func<IndexAsset, int> keySelector, AssetType type)
    {
        ImmutableArray<IndexAsset> assets = GetAssets(type);
        return assets.Length > 0 ? assets.MinBy(keySelector) : null;
    }
    private IndexAsset? MinBy(Func<IndexAsset, int> keySelector, IEnumerable<AssetType> types)
    {
        foreach (AssetType type in types)
        {
            IndexAsset? asset = MinBy(keySelector, type);
            if (asset is not null)
                return asset;
        }

        return null;
    }
}
