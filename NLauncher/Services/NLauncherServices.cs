using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NLauncher.Services.Index;
using NLauncher.Services.Settings;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Services;
public static class NLauncherServices
{
    public static void AddDefault(IServiceCollection services)
    {
        services.AddMudServices();
        services.AddMudMarkdownServices();

        services.AddSingleton<SettingsService>();
        services.AddScoped<IndexService>();
    }

    public static void AddStorage<TStorage>(IServiceCollection services) where TStorage : class, IStorageService
    {
        services.AddSingleton<IStorageService, TStorage>();
    }

    public static void AddAppHandlers(IServiceCollection services, params IEnumerable<AppHandler> handlers)
    {
        services.AddSingleton(new AppHandlerService(handlers));
    }
}
