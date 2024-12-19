using NLauncher.Index.Enums;

namespace NLauncher.Index.Models.Applications.Installs;
public class WebsiteAppInstall : AppInstall
{
    public required Uri Url { get; init; }
    public bool SupportsPwa { get; init; } // defaults to false

    protected override Uri Href => Url;
    public override Platforms GetSupportedPlatforms()
    {
        Platforms platforms = Platforms.Browser;
        if (SupportsPwa)
            platforms |= Platforms.Windows;

        return platforms;
    }
}
