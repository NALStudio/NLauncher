using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using NLauncher.Services.Index;
using NLauncher.Services.Settings;
using NLauncher.Services.Storage;

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

        builder.Services.AddSingleton<IStorageService, WebStorageService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddScoped<IndexService>();

        await builder.Build().RunAsync();
    }
}
