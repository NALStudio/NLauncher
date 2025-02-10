namespace NLauncher.Services.Cache;
internal class CacheEntry
{
    public required byte[] Bytes { get; init; }

    public required int HeaderLength { get; init; }
    public required CacheHeader Header { get; init; }
}
