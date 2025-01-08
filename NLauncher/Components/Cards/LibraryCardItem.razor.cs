using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NLauncher.Components.Menus.LibraryCard;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps;

namespace NLauncher.Components.Cards;
public partial class LibraryCardItem
{
    [Inject]
    private AppLinkPlayService LinkPlayService { get; set; } = default!;

    /// <summary>
    /// Use <see langword="null"/> for skeleton.
    /// </summary>
    [Parameter, EditorRequired]
    public IndexEntry? Entry { get; set; }

    private LibraryCardMenu? menu;
    private AppActionButton? actionButton;

    private string? href;
    private bool mouseInsideMoreButton = false;

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
        menu?.Open(args);
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
}
