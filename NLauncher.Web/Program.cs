using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NLauncher.Services;
using NLauncher.Web.Services;
using System.Runtime.Versioning;

namespace NLauncher.Web;

[SupportedOSPlatform("browser")]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<WebApp>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        NLauncherServices.AddDefault(builder.Services);
        NLauncherServices.AddPlatformInfo<WebPlatformInfoService>(builder.Services);
        NLauncherServices.AddStorage<WebStorageService>(builder.Services);
        NLauncherServices.AddInstalling<WebPlatformInstaller>(builder.Services);

        NLauncherServices.AddAppFiles<WebAppLocalFiles>(builder.Services);
        NLauncherServices.AddAppRunning<WebAppStartup>(builder.Services);
        NLauncherServices.AddGameSessions<WebGameSessionService>(builder.Services);

        await builder.Build().RunAsync();
    }
}
