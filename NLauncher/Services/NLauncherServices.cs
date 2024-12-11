﻿using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NLauncher.Services.Index;
using NLauncher.Services.Library;
using NLauncher.Services.Settings;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        services.AddSingleton<LibraryService>();
    }

    public static void AddStorage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStorage>(IServiceCollection services) where TStorage : class, IStorageService
    {
        services.AddSingleton<IStorageService, TStorage>();
    }

    public static void AddAppHandlers(IServiceCollection services, params IEnumerable<AppHandler> handlers)
    {
        services.AddSingleton(new AppHandlerService(handlers));
    }
}
