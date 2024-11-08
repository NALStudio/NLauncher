using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace NLauncher.Components;

public partial class SettingsMenu
{
    [Parameter]
    public bool Open { get; set; }

    private bool GetDarkMode() => Settings.DarkMode;
    private async Task SetDarkMode(bool darkMode)
    {
        await Settings.UpdateSettings(
            settings => settings.DarkMode = darkMode
        );
        StateHasChanged();
    }

    private string GetDarkModeIcon()
    {
        return GetDarkMode()
            ? Icons.Material.Rounded.DarkMode
            : Icons.Material.Rounded.LightMode;
    }

    public void ToggleOpen()
    {
        Open = !Open;
        StateHasChanged();
    }
}
