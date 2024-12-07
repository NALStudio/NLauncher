using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using NLauncher.Services;
using NLauncher.Shared.AppHandlers.Base;
using NLauncher.Web.Services;
using System.Net.Http.Headers;

namespace NLauncher.Windows;

public partial class MainPage : Form
{
    public MainPage()
    {
        InitializeComponent();

        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = BuildServices();
        blazorWebView.RootComponents.Add<WinApp>("#app");
        blazorWebView.RootComponents.Add<HeadOutlet>("head::after");
    }

    internal static IServiceProvider BuildServices()
    {
        ServiceCollection services = new();

        services.AddWindowsFormsBlazorWebView();

        services.AddScoped(_ => CreateHttp());

        NLauncherServices.AddDefault(services);
        NLauncherServices.AddStorage<WindowsStorageService>(services);
        NLauncherServices.AddAppHandlers(services, AppHandler.WindowsHandlers);

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        return services.BuildServiceProvider();
    }

    private static HttpClient CreateHttp()
    {
        HttpClient http = new();
        http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                ProductHeaderValue.Parse(Constants.UserAgent)
            )
        );

        return http;
    }
}
