using NLauncher.Services.Cache;
using NLauncher.Services.Library;
using NLauncher.Services.Sessions;
using NLauncher.Services.Settings;
using System.Text.Json.Serialization;

namespace NLauncher.Code.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]

[JsonSerializable(typeof(SettingsService.Settings))]
[JsonSerializable(typeof(IEnumerable<LibraryEntry>))]
[JsonSerializable(typeof(LibraryEntry[]))]
[JsonSerializable(typeof(GameSession))]
[JsonSerializable(typeof(CacheHeader))]
public partial class NLauncherJsonContext : JsonSerializerContext;
