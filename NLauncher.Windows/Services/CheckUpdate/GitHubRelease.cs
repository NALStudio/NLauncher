namespace NLauncher.Windows.Services.CheckUpdate;
internal record class GitHubRelease
{
    public required string TagName { get; init; }
}
