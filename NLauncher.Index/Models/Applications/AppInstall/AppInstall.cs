using NLauncher.Index.Enums;
using System.Text.Json.Serialization;

namespace NLauncher.Index.Models.Applications;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public abstract class AppInstall
{
}
