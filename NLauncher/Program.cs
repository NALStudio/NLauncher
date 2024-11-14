using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using NLauncher.Services.Index;
using NLauncher.Services.Settings;
using NLauncher.Services.Storage;
using NLauncher.Shared.AppHandlers;
using NLauncher.Shared.AppHandlers.Base;

namespace NLauncher;
public static class Program
{
    public static async Task Main(string[] args)
    {
        if (!OperatingSystem.IsBrowser())
            throw new PlatformNotSupportedException();

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddMudServices();
        builder.Services.AddMudMarkdownServices();

        builder.Services.AddSingleton<IStorageService, WebStorageService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddScoped<IndexService>();

        builder.Services.AddSingleton(CreateAppHandlerService());

        await builder.Build().RunAsync();
    }

    private static AppHandlerService CreateAppHandlerService()
    {
        return new AppHandlerService([
            ..AppHandler.SharedHandlers
        ]);
    }
}
