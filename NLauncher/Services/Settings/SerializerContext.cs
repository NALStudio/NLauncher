using System.Text.Json.Serialization;

namespace NLauncher.Services.Settings;

public partial class SettingsService
{
    [JsonSerializable(typeof(Settings))]
    private partial class SerializerContext : JsonSerializerContext
    {
    }
}
