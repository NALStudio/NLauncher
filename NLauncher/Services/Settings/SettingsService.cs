using NLauncher.Services.Storage;
using System.Text.Json;

namespace NLauncher.Services.Settings;

public partial class SettingsService
{
    private const string settingsFilename = "settings.json";

    private Settings __settings = Settings.CreateDefault();

    /// <summary>
    /// DO NOT MUTATE!!!
    /// </summary>
    private Settings SettingsRef
    {
        get
        {
            Settings? settings;

            lock (__settings)
            {
                settings = __settings;
            }

            return settings;
        }
    }

    private readonly IStorageService storageService;
    public SettingsService(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public event Action? SettingsChanged;

    /// <summary>
    /// Update the current settings with the values provided by <paramref name="settings"/>.
    /// </summary>
    public async ValueTask UpdateSettings(Settings settings)
    {
        // Save copy so that data can't be mutated by the user
        Settings copy = settings.Copy();

        await InternalSaveSettings(copy);
        InternalUpdateSettings(copy);
    }

    private void InternalUpdateSettings(Settings settingsRef)
    {
        lock (__settings)
        {
            __settings = settingsRef;
        }

        SettingsChanged?.Invoke();
    }

    public async ValueTask UpdateSettings(Action<Settings> updateFunc)
    {
        Settings settings = GetCurrentSettings();
        updateFunc(settings);
        await UpdateSettings(settings); // Save using UpdateSettings so that settings get copied (user can get a reference to the settings during update)
    }

    /// <summary>
    /// Get the current settings.
    /// </summary>
    /// <remarks>
    /// Settings changes are not reflected in the returned settings object.
    /// </remarks>
    public Settings GetCurrentSettings() => SettingsRef.Copy();

    private async ValueTask InternalSaveSettings(Settings settings)
    {
        string json = JsonSerializer.Serialize(settings, SerializerContext.Default.Settings);
        await storageService.WriteAll(settingsFilename, json);
    }

    public async void LoadSettings()
    {
        Settings settings = await InternalLoadSettings() ?? Settings.CreateDefault();
        InternalUpdateSettings(settings);
    }

    private async Task<Settings?> InternalLoadSettings()
    {
        string json = await storageService.ReadAll(settingsFilename);
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<Settings>(json);
    }
}
