using NLauncher.Index.Enums;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Applications.Installs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BinaryAppInstall))]
public abstract class AppInstall
{
    public required InstallType Type { get; init; }
}
