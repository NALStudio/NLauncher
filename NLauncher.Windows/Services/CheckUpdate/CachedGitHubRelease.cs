namespace NLauncher.Windows.Services.CheckUpdate;
internal readonly struct CachedGitHubRelease
{
    public long Timestamp { get; init; }
    public required GitHubRelease Release { get; init; }
}
