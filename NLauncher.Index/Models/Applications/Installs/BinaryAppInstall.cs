using NLauncher.Index.Enums;

namespace NLauncher.Index.Models.Applications.Installs;
public class BinaryAppInstall : AppInstall
{
    public required Platforms Platform { get; init; }
    public required Uri DownloadUrl { get; init; }
    public required string DownloadHash { get; init; }
    public required string ExecutablePath { get; init; }

    protected override Uri Href => DownloadUrl;
    public override Platforms GetSupportedPlatforms() => Platform;
}
