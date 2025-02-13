using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Utilities;
using NLauncher.Components.Menus.LibraryCard;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps;
using NLauncher.Services.Settings;

namespace NLauncher.Components.Cards;
public partial class LibraryCardItem : IDisposable
{
    [Inject]
    private AppLinkPlayService LinkPlayService { get; set; } = default!;

    [Inject]
    private SettingsService Settings { get; set; } = default!;

    /// <summary>
    /// Use <see langword="null"/> for skeleton.
    /// </summary>
    [Parameter, EditorRequired]
    public IndexEntry? Entry { get; set; }

    private LibraryCardMenu? menu;
    private AppActionButton? actionButton;

    private string? href;
    private bool mouseInsideMoreButton = false;

    private bool darkMode;

    private string LibraryCardOverlayClasses => new CssBuilder().AddClass("library_card_overlay")
                                                                .AddClass("library_card_overlay_dark", darkMode)
                                                                .AddClass("library_card_overlay_light", !darkMode)
                                                                .Build();

    protected override void OnInitialized()
    {
        darkMode = Settings.DarkMode;
        Settings.SettingsChanged += SettingsChanged;
    }

    private void SettingsChanged()
    {
        if (Settings.DarkMode != darkMode)
        {
            darkMode = Settings.DarkMode;
            StateHasChanged();
        }
    }

    protected override void OnParametersSet()
    {
        AppManifest? app = Entry?.Manifest;

        if (app is not null && LinkPlayService.CanPlay(app))
            href = LinkPlayService.TryGetPrimaryOption(app)?.GetHref();
        else
            href = null;
    }

    private async Task OnCardPressed()
    {
        if (Entry is null)
            return;

        await actionButton!.Activate();
    }

    private void OpenContextMenu(MouseEventArgs args)
    {
        menu?.OpenMenuAsync(args);
    }

    private static string GetBackgroundImageCss(IndexAsset? asset)
    {
        if (asset is null)
            return string.Empty;

        return $"background-image: url(\"{asset.Url}\")";
    }

    private void MouseEnteredMoreButton()
    {
        mouseInsideMoreButton = true;
        StateHasChanged();
    }

    private void MouseLeftMoreButton()
    {
        mouseInsideMoreButton = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        Settings.SettingsChanged -= SettingsChanged;
    }
}
