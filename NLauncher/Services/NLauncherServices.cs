using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NLauncher.Services.Apps;
using NLauncher.Services.Index;
using NLauncher.Services.Library;
using NLauncher.Services.Settings;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.Services;
public static class NLauncherServices
{
    // includeMudBlazor is not currently used, but implemented in case it is needed in the future
    public static void AddDefault(IServiceCollection services, bool includeMudBlazor = true)
    {
        if (includeMudBlazor)
        {
            services.AddMudServices();
            services.AddMudMarkdownServices();
        }

        services.AddSingleton<AppBarMenus>();

        services.AddSingleton<SettingsService>();
        services.AddScoped<IndexService>();
        services.AddSingleton<LibraryService>();

        services.AddSingleton<AppInstallService>();
        services.AddSingleton<AppLinkPlayService>();
    }

    public static void AddInstalling<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInstaller>(IServiceCollection services) where TInstaller : class, IPlatformInstaller
    {
        services.AddSingleton<IPlatformInstaller, TInstaller>();
    }

    public static void AddStorage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStorage>(IServiceCollection services) where TStorage : class, IStorageService
    {
        services.AddSingleton<IStorageService, TStorage>();
    }
}
