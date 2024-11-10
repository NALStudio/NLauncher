using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Storage;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace NLauncher.Services.Index;

public partial class IndexService
{
    private const string cacheFilename = "indexCache.json";
    private const string indexManifestEndpoint = "https://api.github.com/repos/NALStudio/NLauncher-Index/contents/indexmanifest.json";

    private readonly ILogger<IndexService> logger;
    private readonly HttpClient http;
    private readonly IStorageService storageService;
    public IndexService(ILogger<IndexService> logger, HttpClient http, IStorageService storageService)
    {
        this.logger = logger;
        this.http = http;
        this.storageService = storageService;
    }

    private readonly object getIndexLock = new();
    private Task<CachedIndex>? getIndex;

    public async Task<IndexManifest> GetIndexAsync()
    {
        Task<CachedIndex> cacheTask;
        lock (getIndexLock)
        {
            getIndex ??= GetIndexThreaded(useCache: true);
            cacheTask = getIndex;
        }

        CachedIndex cached = await cacheTask;
        if (IsCacheOutdated(cached))
        {
            Task<CachedIndex> refreshTask;
            lock (getIndexLock)
            {
                getIndex = GetIndexThreaded(useCache: false);
                refreshTask = getIndex;
            }

            CachedIndex refreshed = await refreshTask;
            Debug.Assert(!IsCacheOutdated(refreshed));
            return refreshed.Index;
        }
        else
        {
            return cached.Index;
        }
    }

    private static bool IsCacheOutdated(CachedIndex cached)
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > cached.ExpiresTimestamp;
    }

    private async ValueTask<CachedIndex?> TryGetCachedIndex()
    {
        string serialized = await storageService.ReadAll(cacheFilename);
        if (string.IsNullOrEmpty(serialized))
            return null;

        CachedIndex? deserialized = CachedIndex.TryDeserialize(serialized);
        if (!deserialized.HasValue)
            logger.LogWarning("Cache value could not be deserialized.");
        return deserialized;
    }

    private async Task<CachedIndex> GetIndexThreaded(bool useCache)
    {
        return await Task.Run(() => GetIndex(useCache));
    }

    private async Task<CachedIndex> GetIndex(bool useCache)
    {
        if (useCache)
        {
            CachedIndex? cached = await TryGetCachedIndex();
            if (cached.HasValue)
            {
                logger.LogInformation("Index loaded from cache.");
                return cached.Value;
            }
        }

        IndexManifest index = await FetchIndex();
        logger.LogInformation("Index downloaded from repository.");
        return await SaveIndexToCache(index);
    }

    private async ValueTask<CachedIndex> SaveIndexToCache(IndexManifest index, long expiresIn = 86400) // 86400 seconds == 1 day
    {
        CachedIndex cached = new()
        {
            ExpiresTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn,
            Index = index
        };

        string serialized = CachedIndex.Serialize(cached);
        await storageService.WriteAll(cacheFilename, serialized);

        return cached;
    }

    private async Task<IndexManifest> FetchIndex()
    {
        HttpRequestMessage request = new()
        {
            RequestUri = new Uri(indexManifestEndpoint),
            Method = HttpMethod.Get
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.raw+json"));

        HttpResponseMessage response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        IndexManifest? manifest;
        await using (Stream s = response.Content.ReadAsStream())
        {
            manifest = await IndexJsonSerializer.DeserializeAsync<IndexManifest>(s);
        }

        return manifest ?? throw new InvalidOperationException("Could not deserialize index manifest.");
    }
}
