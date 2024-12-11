using NLauncher.Index.Json;
using NLauncher.Index.Models;
using NLauncher.Services.Index;
using NLauncher.Services.Library;
using NLauncher.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Code.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]

[JsonSerializable(typeof(SettingsService.Settings))]
[JsonSerializable(typeof(Dictionary<Guid, LibraryEntry>))]
internal partial class NLauncherJsonContext : JsonSerializerContext
{
}
