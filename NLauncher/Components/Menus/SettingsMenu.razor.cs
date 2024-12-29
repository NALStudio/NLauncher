using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace NLauncher.Components.Menus;

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

    private string GetDarkModeText()
    {
        return GetDarkMode() ? "Dark Mode" : "Light Mode";
    }
    private string GetDarkModeIcon()
    {
        return GetDarkMode()
            ? Icons.Material.Rounded.DarkMode
            : Icons.Material.Rounded.LightMode;
    }
}
