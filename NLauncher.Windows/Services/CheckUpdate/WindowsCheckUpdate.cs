using Microsoft.Extensions.Logging;
using NLauncher.Services.Cache;
using NLauncher.Services.CheckUpdate;
using NLauncher.Windows.Json;
using System.Net.Http.Json;

namespace NLauncher.Windows.Services.CheckUpdate;
internal class WindowsCheckUpdate : ICheckUpdate
{
    private const string kEndpoint = "https://api.github.com/repos/NALStudio/NLauncher/releases/latest";

    private readonly ILogger<WindowsCheckUpdate> logger;
    private readonly HttpClient http;
    private readonly CacheService cache;

    public WindowsCheckUpdate(ILogger<WindowsCheckUpdate> logger, HttpClient http, CacheService cache)
    {
        this.logger = logger;
        this.http = http;
        this.cache = cache;
    }

    public async ValueTask<AvailableUpdate?> CheckForUpdateAsync()
    {
        GitHubRelease? release = await cache.TryRead(Constants.LatestReleaseCacheKey, WindowsJsonContext.Default.GitHubRelease);
        release ??= await FetchAndCacheLatestRelease();

        if (release is not null)
            return CheckUpdate(release);
        else
            return null;
    }

    private async ValueTask<GitHubRelease?> FetchAndCacheLatestRelease()
    {
        HttpResponseMessage response = await http.GetAsync(kEndpoint);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Update fetch failed with error code: {}.", response.StatusCode);
            return null;
        }

        GitHubRelease? release = await response.Content.ReadFromJsonAsync(WindowsJsonContext.Default.GitHubRelease);
        if (release is null)
        {
            logger.LogError("Could not deserialize release.");
            return null;
        }

        logger.LogInformation("Fetched latest release from GitHub.");

        await cache.Write(Constants.LatestReleaseCacheKey, release, WindowsJsonContext.Default.GitHubRelease, validFor: TimeSpan.FromDays(1));

        return release;
    }

    private AvailableUpdate? CheckUpdate(GitHubRelease release)
    {
        string latestVersion = release.TagName;
        string currentVersion = Application.ProductVersion.Split('+', 2)[0];

        if (currentVersion != latestVersion)
        {
            return new AvailableUpdate()
            {
                AvailableVersion = latestVersion,
                CurrentVersion = currentVersion
            };
        }
        else
        {
            return null;
        }
    }
}
