namespace NLauncher.Services;
public class AppBarMenus
{
    public bool SettingsOpen { get; private set; }
    public bool DownloadsOpen { get; private set; }

    public bool AnyMenuOpen => SettingsOpen || DownloadsOpen;

    public event Action? OnChanged;

    public void ToggleSettings() => OpenSettings(!SettingsOpen);
    public void OpenSettings(bool open = true)
    {
        Close();
        SettingsOpen = open;
        OnChanged?.Invoke();
    }

    public void ToggleDownloads() => OpenDownloads(!DownloadsOpen);
    public void OpenDownloads(bool open = true)
    {
        Close();
        DownloadsOpen = open;
        OnChanged?.Invoke();
    }

    public void CloseAll()
    {
        Close();
        OnChanged?.Invoke();
    }

    private void Close()
    {
        SettingsOpen = false;
        DownloadsOpen = false;
    }
}
