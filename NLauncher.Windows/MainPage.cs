using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using NLauncher.Services;
using NLauncher.Windows.Services;
using NLauncher.Windows.Services.Apps;
using NLauncher.Windows.Services.GameSessions;

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

        blazorWebView.BlazorWebViewInitializing += WebViewInitializationStarted;

        // WebView title changed reference: https://github.com/MicrosoftEdge/WebView2Feedback/issues/1816
        blazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;
    }

    private void WebViewInitializationStarted(object? sender, BlazorWebViewInitializingEventArgs e)
    {
        e.UserDataFolder = Path.Join(WindowsStorageService.AppDataPath, Constants.WebViewUserDataFolder);
    }

    private void CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (e.IsSuccess)
            blazorWebView.WebView.CoreWebView2.DocumentTitleChanged += (_, _) => UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        string title = blazorWebView.WebView.CoreWebView2.DocumentTitle;

        // During webview initialization, webview spews some random titles.
        // We don't want these random titles to flicker on the window itself so we'll only change titles if they're valid.
        // We check this each time because WebView first shows the title from the <head>, then random gibberish and then again the title from <head> which is dumb as fuck
        if (title.StartsWith("NLauncher", StringComparison.OrdinalIgnoreCase))
            Text = title;
    }

    private static IServiceProvider BuildServices()
    {
        ServiceCollection services = new();
        services.AddLogging(builder =>
        {
#if DEBUG
            builder.AddDebug();
#endif
        });

        services.AddWindowsFormsBlazorWebView();

        services.AddScoped(_ => Program.HttpClient);

        NLauncherServices.AddDefault(services);
        NLauncherServices.AddPlatformInfo<WindowsPlatformInfoService>(services);
        NLauncherServices.AddStorage<WindowsStorageService>(services);
        NLauncherServices.AddInstalling<WindowsPlatformInstaller>(services);

        NLauncherServices.AddAppFiles<WindowsAppLocalFiles>(services);
        NLauncherServices.AddAppRunning<WindowsAppStartup>(services);
        NLauncherServices.AddGameSessions<WindowsGameSessionService>(services);

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        return services.BuildServiceProvider();
    }
}
