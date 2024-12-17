using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using NLauncher.Services;
using NLauncher.Shared.AppHandlers.Base;
using NLauncher.Web.Services;
using System.Net.Http.Headers;

namespace NLauncher.Windows;

public partial class MainPage : Form
{
    private const string defaultTitle = "NLauncher";
    private bool canChangeTitle = false;

    public MainPage()
    {
        InitializeComponent();

        Text = defaultTitle;

        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = BuildServices();
        blazorWebView.RootComponents.Add<WinApp>("#app");
        blazorWebView.RootComponents.Add<HeadOutlet>("head::after");

        // WebView title changed reference: https://github.com/MicrosoftEdge/WebView2Feedback/issues/1816
        blazorWebView.WebView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
    }

    private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (e.IsSuccess)
            blazorWebView.WebView.CoreWebView2.DocumentTitleChanged += (_, _) => UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        string title = blazorWebView.WebView.CoreWebView2.DocumentTitle;

        // During webview initialization, webview spews a bunch of random titles.
        // We don't want these random titles to flicker on the window itself so we'll just wait until a proper title is provided.
        if (title.StartsWith(defaultTitle, StringComparison.OrdinalIgnoreCase))
            canChangeTitle = true;

        if (canChangeTitle)
            Text = title;
    }

    private static IServiceProvider BuildServices()
    {
        ServiceCollection services = new();

        services.AddWindowsFormsBlazorWebView();

        services.AddScoped(_ => CreateHttp());

        NLauncherServices.AddDefault(services);
        NLauncherServices.AddStorage<WindowsStorageService>(services);
        NLauncherServices.AddAppHandling(services, AppHandler.WindowsHandlers);

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
