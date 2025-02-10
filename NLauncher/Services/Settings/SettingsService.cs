using Microsoft.Extensions.Logging;
using NLauncher.Code;
using NLauncher.Code.Json;
using System.Text.Json;

namespace NLauncher.Services.Settings;

public partial class SettingsService
{
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

    private readonly ILogger<SettingsService> logger;
    private readonly IStorageService storageService;
    public SettingsService(ILogger<SettingsService> logger, IStorageService storageService)
    {
        this.logger = logger;
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
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(settings, NLauncherJsonContext.Default.Settings);
        await storageService.Write(NLauncherConstants.FileNames.Settings, json);
    }

    public async void LoadSettings()
    {
        Settings? settings;
        try
        {
            settings = await InternalLoadSettings();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Settings could not be deserialized.");
            settings = null;
        }

        settings ??= Settings.CreateDefault();
        InternalUpdateSettings(settings);
    }

    private async Task<Settings?> InternalLoadSettings()
    {
        byte[] json = await storageService.ReadUtf8(NLauncherConstants.FileNames.Settings);
        if (json.Length == 0)
            return null;

        return JsonSerializer.Deserialize(json, NLauncherJsonContext.Default.Settings);
    }
}
