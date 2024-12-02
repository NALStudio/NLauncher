using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NLauncher.Services;
using NLauncher.Shared.AppHandlers.Base;
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
        NLauncherServices.AddStorage<WebStorageService>(builder.Services);
        NLauncherServices.AddAppHandlers(builder.Services, AppHandler.WebHandlers);

        await builder.Build().RunAsync();
    }
}
