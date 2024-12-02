using Microsoft.Extensions.Logging;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace NLauncher.Services.Index;

public partial class IndexService : IDisposable
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

    private readonly object cachedLock = new();
    private CachedIndex? cached;

    private readonly SemaphoreSlim loadLock = new(1, 1);

    public bool TryGetCachedIndex([MaybeNullWhen(false)] out IndexManifest manifest)
    {
        CachedIndex? cached;
        lock (cachedLock)
        {
            cached = this.cached;
        }

        if (IsCacheValid(cached))
        {
            manifest = cached.Value.Index;
            return true;
        }
        else
        {
            manifest = null;
            return false;
        }
    }

    public async ValueTask<IndexManifest> GetIndexAsync()
    {
        // Try to get cached index first
        if (TryGetCachedIndex(out IndexManifest? cached))
            return cached;

        // Wait for our turn to load the index
        await loadLock.WaitAsync();

        CachedIndex loaded;
        try
        {
            // An earlier load task might've already loaded a valid index to cache.
            // If a valid index is found, return that instead and don't bother to load a new one.
            if (TryGetCachedIndex(out IndexManifest? recheckedCache))
                return recheckedCache;

            loaded = await Task.Run(LoadCacheAsync);
            lock (cachedLock)
            {
                this.cached = loaded;
            }
        }
        finally
        {
            loadLock.Release();
        }

        return loaded.Index;
    }

    // Task instead of ValueTask so that we can call it directly in Task.Run instead of using a wrapper function
    private async Task<CachedIndex> LoadCacheAsync()
    {
        // Try to load cached value from storage
        CachedIndex? cached = await LoadIndexFromCache();
        if (IsCacheValid(cached))
        {
            logger.LogInformation("Loaded index from cache.");
            return cached.Value;
        }

        // No caches left, fetch from GitHub
        IndexManifest fetched = await FetchIndexFromGitHub();
        CachedIndex cachedFetch = await SaveIndexToCache(fetched);

        logger.LogInformation("Fetched index from GitHub.");
        Debug.Assert(IsCacheValid(cachedFetch));
        return cachedFetch;
    }

    private static bool IsCacheValid([NotNullWhen(true)] CachedIndex? cached)
    {
        static bool IsCacheOutdated(CachedIndex cached) => DateTimeOffset.UtcNow.ToUnixTimeSeconds() > cached.ExpiresTimestamp;

        if (!cached.HasValue)
            return false;
        if (IsCacheOutdated(cached.Value))
            return false;

        return true;
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

    private async ValueTask<CachedIndex?> LoadIndexFromCache()
    {
        string serialized = await storageService.ReadAll(cacheFilename);
        if (string.IsNullOrEmpty(serialized))
            return null;

        CachedIndex? deserialized = CachedIndex.TryDeserialize(serialized);
        if (!deserialized.HasValue)
            logger.LogWarning("Cache value could not be deserialized.");
        return deserialized;
    }

    private async Task<IndexManifest> FetchIndexFromGitHub()
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

    public void Dispose()
    {
        loadLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
