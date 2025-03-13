using NLauncher.Windows.Commands.Install;
using NLauncher.Windows.Commands.Run;
using NLauncher.Windows.Commands.Uninstall;
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
        if (args.Length > 0 && args[0] == "command")
        {
            return RunConsole(args[1..]);
        }
        else
        {
            RunWinForms();
            return 0;
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
    private static int RunConsole(IEnumerable<string> args)
    {
        string logPath = Path.Join(Constants.GetAppDataDirectory(), string.Format(Constants.CommandLogFileNameTemplate, Guid.NewGuid()));
        StreamWriter? logWriter;
        try
        {
            logWriter = new StreamWriter(File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true
            };
            Console.SetOut(logWriter);
        }
        catch (Exception ex)
        {
            logWriter = null;
            Console.WriteLine("Could not redirect stdout. Error below:");
            Console.WriteLine(ex.ToString());
        }


        int? result = null;
        try
        {
            Console.WriteLine($"Running command: '{Environment.CommandLine}'");

            CommandApp app = new();

            app.Configure(config =>
            {
                config.AddBranch<InstallSettings>("install", install =>
                {
                    install.AddCommand<BinaryInstallCommand>("binary");
                });
                config.AddBranch<UninstallSettings>("uninstall", uninstall =>
                {
                    uninstall.AddCommand<BinaryUninstallCommand>("binary");
                });
                config.AddBranch<RunSettings>("run", run =>
                {
                    run.AddCommand<BinaryRunCommand>("binary");
                });
            });

            result = app.Run(args);
        }
        finally
        {
            bool deleteLogFile;
#if DEBUG
            deleteLogFile = result != 0;
#else
            deleteLogFile = true;
#endif

            if (deleteLogFile && logWriter is not null)
            {
                logWriter.Dispose();
                File.Delete(logPath);
            }
        }

        return result ?? -1;
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