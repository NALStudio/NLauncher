using Microsoft.Extensions.Logging;
using NLauncher.Code;
using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Cache;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NLauncher.Services.Index;

/// <summary>
/// <see cref="IndexService"/> is multiprocessing safe-ish.
/// </summary>
public partial class IndexService : IDisposable
{
    private readonly ILogger<IndexService> logger;
    private readonly HttpClient http;
    private readonly CacheService cache;
    public IndexService(ILogger<IndexService> logger, HttpClient http, CacheService cache)
    {
        this.logger = logger;
        this.http = http;
        this.cache = cache;
    }

    // private readonly Lock cachedLock = new();
    private IndexManifest? cached;

    private readonly SemaphoreSlim loadLock = new(1, 1);

    public bool TryGetCachedIndex([MaybeNullWhen(false)] out IndexManifest manifest)
    {
        manifest = cached;
        return manifest is not null;

        /*
        IndexManifest? cached;
        lock (cachedLock)
        {
            cached = this.cached;
        }

        if (cached?.Header.IsExpired == false)
        {
            manifest = cached.Value;
            return true;
        }
        else
        {
            manifest = null;
            return false;
        }
        */
    }

    public async ValueTask<IndexManifest> GetIndexAsync()
    {
        // Try to get cached index first
        if (TryGetCachedIndex(out IndexManifest? cached))
            return cached;

        // Wait for our turn to load the index
        await loadLock.WaitAsync();

        IndexManifest loaded;
        try
        {
            // An earlier load task might've already loaded a valid index to cache.
            // If a valid index is found, return that instead and don't bother to load a new one.
            if (TryGetCachedIndex(out IndexManifest? recheckedCache))
                return recheckedCache;

            loaded = await Task.Run(LoadCacheAsync);
            // lock (cachedLock)
            this.cached = loaded;
        }
        finally
        {
            loadLock.Release();
        }

        return loaded;
    }

    // Task instead of ValueTask so that we can call it directly in Task.Run instead of using a wrapper function
    private async Task<IndexManifest> LoadCacheAsync()
    {
        // Try to load cached value from storage
        IndexManifest? cached = await cache.TryRead(NLauncherConstants.CacheNames.Index, IndexJsonContext.Default.IndexManifest);
        if (ValidateCachedIndexManifest(cached))
        {
            logger.LogInformation("Loaded index from cache.");
            return cached;
        }

        // No caches left, fetch from GitHub
        IndexManifest fetched = await FetchIndexFromGitHub();
        await cache.Write(NLauncherConstants.CacheNames.Index, fetched, IndexJsonContext.Default.IndexManifest, validFor: TimeSpan.FromDays(1));

        logger.LogInformation("Fetched index from GitHub.");
        return fetched;
    }

    /// <summary>
    /// Validates that the cached manifest exists and it isn't of a different environment.
    /// </summary>
    private static bool ValidateCachedIndexManifest([NotNullWhen(true)] IndexManifest? manifest)
    {
        if (manifest is null)
            return false;

        if (manifest.Environment != IndexEnvironmentEnum.GetCurrentEnvironment())
            return false;

        return true;
    }

    private async Task<IndexManifest> FetchIndexFromGitHub()
    {
        HttpRequestMessage request = new()
        {
            RequestUri = new Uri(NLauncherConstants.IndexManifestUrl),
            Method = HttpMethod.Get
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.raw+json"));

        HttpResponseMessage response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        IndexManifest? manifest;
        await using (Stream s = response.Content.ReadAsStream())
        {
            manifest = await JsonSerializer.DeserializeAsync(s, IndexJsonContext.Default.IndexManifest);
        }

        return manifest ?? throw new InvalidOperationException("Could not deserialize index manifest.");
    }

    public void Dispose()
    {
        loadLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
