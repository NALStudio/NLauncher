using MudBlazor;
using NLauncher.Components.Dialogs.Installs.Choose;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Apps;
public class AppLinkPlayService
{
    /// <summary>
    /// Returns an href to the primary website if one can be determined from the link handler options.
    /// </summary>
    public AppInstall? TryGetPrimaryOption(AppManifest app)
    {
        AppInstall[] installs = GetLinkPlayInstalls(app).ToArray();

        WebsiteAppInstall? webApp = GetSingleOrNull<WebsiteAppInstall>(installs);
        if (webApp is not null)
            return webApp;

        StoreLinkAppInstall? storeLink = GetSingleOrNull<StoreLinkAppInstall>(installs);
        if (storeLink is not null)
            return storeLink;

        // More can be added in the same fashion as above

        return null;
    }

    /// <summary>
    /// Returns a single value of type <typeparamref name="T"/> in <paramref name="installs"/> or <see langword="null"/> if either none or more than 1 are found.
    /// </summary>
    private static T? GetSingleOrNull<T>(ICollection<AppInstall> installs) where T : AppInstall
    {
        T? value = null;
        foreach (AppInstall install in installs)
        {
            if (install is T ins)
            {
                if (value is null)
                    value = ins;
                else
                    return null;
            }
        }

        return value;
    }

    public async Task Play(AppManifest app, IDialogService dialogService)
    {
        _ = await ChooseInstallDialog.ShowLinkAsync(dialogService, GetLinkPlayInstalls(app));
    }

    public bool CanPlay(AppManifest app) => GetLinkPlayInstalls(app).Any();
    public static bool CanPlay(AppInstall ins) => ins is WebsiteAppInstall or StoreLinkAppInstall;

    private static IEnumerable<AppInstall> GetLinkPlayInstalls(AppManifest app)
    {
        // Link play always uses the latest version
        AppVersion? version = app.GetLatestVersion();
        if (version is null)
            return Enumerable.Empty<AppInstall>();

        return version.Installs.Where(CanPlay);
    }
}
