using NLauncher.Index.Enums;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Applications.Installs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BinaryAppInstall), "binary")]
[JsonDerivedType(typeof(WebsiteAppInstall), "website")]
[JsonDerivedType(typeof(StoreLinkAppInstall), "storelink")]
public abstract class AppInstall
{
    // Uri instead of string so that I can quickly verify that the link is absolute and hopefully not malware
    protected abstract Uri Href { get; }
    public abstract Platforms GetSupportedPlatforms();

    public Uri GetHrefUri()
    {
        Uri href = Href;
        if (!href.IsAbsoluteUri)
            throw new InvalidOperationException("Href access forbidden due to security. Uri is not absolute and thus might be malicious.");
        return href;
    }
    public string GetHref() => GetHrefUri().ToString();

    /// <summary>
    /// Returns <see langword="true"/> if automatic installation is supported by <paramref name="platform"/>.
    /// </summary>
    public virtual bool SupportsAutomaticInstall(Platforms platform) => false;
}
