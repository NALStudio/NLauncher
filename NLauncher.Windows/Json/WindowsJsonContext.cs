using NLauncher.Windows.Services.CheckUpdate;
using System.Text.Json.Serialization;

namespace NLauncher.Windows.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease))]
internal partial class WindowsJsonContext : JsonSerializerContext
{
}
