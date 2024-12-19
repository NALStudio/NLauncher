using NLauncher.Index.Enums;

namespace NLauncher.Index.Models.Applications.Installs;
public class StoreLinkAppInstall : AppInstall
{
    public required Platforms Platform { get; init; }
    public required Uri Url { get; init; }

    protected override Uri Href => Url;
    public override Platforms GetSupportedPlatforms() => Platform;
}
