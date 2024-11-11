using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLauncher.Services.Settings;

public partial class SettingsService
{
    public class Settings
    {
        public required bool DarkMode { get; set; }

        public static Settings CreateDefault()
        {
            return new()
            {
                DarkMode = true
            };
        }

        public Settings Copy()
        {
            return new()
            {
                DarkMode = DarkMode
            };
        }
    }
}
