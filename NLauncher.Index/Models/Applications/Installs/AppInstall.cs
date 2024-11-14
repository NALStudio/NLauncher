using NLauncher.Index.Enums;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Applications.Installs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BinaryAppInstall), "binary_exe")]
[JsonDerivedType(typeof(WebsiteAppInstall), "website")]
public abstract class AppInstall
{
    public abstract Platforms GetSupportedPlatforms();
}
