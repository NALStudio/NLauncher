using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using NLauncher.Services.Index;
using NLauncher.Services.Settings;
using NLauncher.Services.Storage;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NLauncher.Desktop;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddScoped(static _ => CreateHttpClient());

        builder.Services.AddMudServices();
        builder.Services.AddMudMarkdownServices();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            builder.Services.AddSingleton<IStorageService, WindowsStorageService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddScoped<IndexService>();

        builder.Services.AddSingleton(new AppHandlerService(AppHandler.SharedHandlers));

        return builder.Build();
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient http = new();
        http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(GetDefaultUserAgent())
        );

        return http;
    }

    private static ProductHeaderValue GetDefaultUserAgent()
    {
        return new ProductHeaderValue(
            name: AppInfo.Current.Name,
            version: AppInfo.Current.VersionString
        );
    }
}
