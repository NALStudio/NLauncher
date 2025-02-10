using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NLauncher.Services.Apps;
using NLauncher.Services.Apps.Installing;
using NLauncher.Services.Apps.Running;
using NLauncher.Services.Cache;
using NLauncher.Services.CheckUpdate;
using NLauncher.Services.Index;
using NLauncher.Services.Library;
using NLauncher.Services.Sessions;
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

        services.AddScoped<AppBarMenus>();

        services.AddScoped<SettingsService>();
        services.AddScoped<CacheService>();
        services.AddScoped<IndexService>();
        services.AddScoped<LibraryService>();

        services.AddScoped<AppLinkPlayService>();
    }

    public static void AddPlatformInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPlatformInfo>(IServiceCollection services) where TPlatformInfo : class, IPlatformInfoService
    {
        services.AddScoped<IPlatformInfoService, TPlatformInfo>();
    }

    public static void AddStorage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStorage>(IServiceCollection services) where TStorage : class, IStorageService
    {
        services.AddScoped<IStorageService, TStorage>();
    }

    public static void AddUpdateCheck<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCheck>(IServiceCollection services) where TCheck : class, ICheckUpdate
    {
        services.AddScoped<ICheckUpdate, TCheck>();
    }

    public static void AddInstalling<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInstaller>(IServiceCollection services) where TInstaller : class, IPlatformInstaller
    {
        services.AddScoped<IPlatformInstaller, TInstaller>();
        services.AddScoped<AppInstallService>();
        services.AddScoped<AppUninstallService>();
    }

    public static void AddAppFiles<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAppFiles>(IServiceCollection services) where TAppFiles : class, IAppLocalFiles
    {
        services.AddScoped<IAppLocalFiles, TAppFiles>();
    }

    public static void AddAppRunning<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAppStartup>(IServiceCollection services) where TAppStartup : class, IAppStartup
    {
        services.AddScoped<IAppStartup, TAppStartup>();
        services.AddScoped<AppRunningService>();
    }

    public static void AddGameSessions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGameSessions>(IServiceCollection services) where TGameSessions : class, IGameSessionService
    {
        services.AddScoped<IGameSessionService, TGameSessions>();
    }
}
