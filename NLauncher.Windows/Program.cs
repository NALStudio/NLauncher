using NLauncher.Windows.Commands;
using NLauncher.Windows.Commands.Install;
using Spectre.Console.Cli;
using System.Net.Http.Headers;

namespace NLauncher.Windows;

internal static class Program
{
    public static HttpClient HttpClient { get; } = CreateHttp();

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            RunWinForms();
            return 0;
        }
        else
        {
            return RunConsole(args);
        }
    }

    private static void RunWinForms()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainPage());
    }

    /// <summary>
    /// This method blocks the thread until all tasks are complete.
    /// </summary>
    private static int RunConsole(string[] args)
    {
        CommandApp app = new();

        app.Configure(config =>
        {
            config.AddCommand<InstallCommand>("install");
            config.AddCommand<RunCommand>("run");
        });

        return app.Run(args);
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