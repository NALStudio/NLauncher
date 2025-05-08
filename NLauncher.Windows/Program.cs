using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLauncher.Services;
using NLauncher.Windows.Commands.Default.Install;
using NLauncher.Windows.Commands.Default.Run;
using NLauncher.Windows.Commands.Default.Uninstall;
using NLauncher.Windows.Commands.Protoc;
using NLauncher.Windows.Services;
using NLauncher.Windows.Services.Apps;
using NLauncher.Windows.Services.CheckUpdate;
using NLauncher.Windows.Services.GameSessions;
using NLauncher.Windows.Services.Installing;
using NReco.Logging.File;
using Spectre.Console;
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
        string? arg0 = args.Length > 0 ? args[0] : null;
        string[] argsRest = args.Length > 1 ? args[1..] : Array.Empty<string>();

        switch (arg0)
        {
            case "command":
                return RunConsole(argsRest).Result;
            case "protoc":
                return RunUrlProtocol(argsRest).Result;
            default:
                RunWinForms(args);
                return 0;
        }
    }

    private static string EnsureSafePagePath(string path)
    {
        // Ensure that the path starts with a slash, so we don't escape out of NLauncher URLs

        if (path.StartsWith('/'))
            return path;
        else
            return "/" + path;
    }

    private static ServiceProvider BuildServices(string? logPath)
    {
        ServiceCollection services = new();
        services.AddLogging(builder =>
        {
#if DEBUG
            builder.AddDebug();
#endif

            if (logPath is not null)
                builder.AddFile(Path.Join(Constants.GetAppDataDirectory(), logPath), append: false);
        });

        services.AddWindowsFormsBlazorWebView();

        services.AddScoped(_ => Program.HttpClient);

        NLauncherServices.AddDefault(services);
        NLauncherServices.AddPlatformInfo<WindowsPlatformInfoService>(services);
        NLauncherServices.AddStorage<WindowsStorageService>(services);
        NLauncherServices.AddUpdateCheck<WindowsCheckUpdate>(services);
        NLauncherServices.AddInstalling<WindowsPlatformInstaller>(services);

        NLauncherServices.AddAppFiles<WindowsAppLocalFiles>(services);
        NLauncherServices.AddAppRunning<WindowsAppStartup>(services);
        NLauncherServices.AddGameSessions<WindowsGameSessionService>(services);

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        return services.BuildServiceProvider();
    }

    private static void RunWinForms(string[] args)
    {
        // We only do very limited input handling so that our URL handling won't be an attack vector to the app
        string? path = null;
        if (args.Length > 1 && args[0] == "--page")
            path = EnsureSafePagePath(args[1]);

        ServiceProvider services = BuildServices(Constants.LauncherLogFileName);
        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainPage(path, services));
        }
        finally
        {
            services.DisposeAsync().Preserve().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// This method blocks the thread until all tasks are complete.
    /// </summary>
    private static async Task<int> RunConsole(IEnumerable<string> args)
    {
        StreamWriter? logWriter = await RedirectOutputToLogAsync(debugOnly: true);

        int? result = null;
        try
        {
            Console.WriteLine($"Running command: '{Environment.CommandLine}'");
            result = await BuildDefaultCommandApp().RunAsync(args);
            return result.Value;
        }
        finally
        {
            await DisposeLogWriterAsync(logWriter, deleteLog: result == 0);
        }
    }

    private static async Task<int> RunUrlProtocol(string[] args)
    {
#if DEBUG
        // ?? null to force the linter to treat logPath as possibly null.
        string? logPath = string.Format(Constants.ProtocolLogFileNameTemplate, Guid.NewGuid()) ?? null;
#else
        const string? logPath = null;
#endif

        Exception? exception = null;
        ProtocError? error = null;
        ServiceProvider? services = null;
        try
        {
            services = BuildServices(logPath);

            if (args.Length == 1 && Uri.TryCreate(args[0], UriKind.Absolute, out Uri? uri) && uri.IsWellFormedOriginalString())
            {
                string auth = uri.Authority;
                ProtocCommand? command = auth switch
                {
                    "rungameid" => new RunGameIdCommand(),
                    _ => null
                };

                if (command is not null)
                {

                    string[] cmdArgs = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    ProtocError? err = await command.ExecuteAsync(services, cmdArgs);
                    if (err is not null)
                        error = err;
                }
                else
                {
                    error = $"Command not found: '{auth}'";
                }
            }
            else
            {
                error = "URL path could not be parsed into arguments.";
            }

            if (error is not null)
                MessageBox.Show(error.Message, "Failed To Run Command", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            error = "Internal error occured.";
            exception = ex;
            Console.WriteLine(ex);
        }
        finally
        {
            try
            {
                if (logPath is not null && exception is null && error is null)
                    File.Delete(logPath);
            }
            catch (Exception ex)
            {
                services?.GetRequiredService<ILogger>().LogError(ex, "Could not delete log file.");
            }

            if (services is not null)
                await services.DisposeAsync();
        }

        if (exception is not null)
            return -1;
        else if (error is not null)
            return 1;
        else
            return 0;
    }

    public static CommandApp BuildDefaultCommandApp()
    {
        CommandApp app = new();

        app.Configure(config =>
        {
            config.SetExceptionHandler((ex, _) =>
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.Default);
                return -1;
            });

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

        return app;
    }

#pragma warning disable RCS1163 // debugOnly is used in release mode
    private static async ValueTask<StreamWriter?> RedirectOutputToLogAsync(bool debugOnly)
#pragma warning restore RCS1163
    {
#if !DEBUG
        if (debugOnly)
            return null;
#endif

        string logPath = Path.Join(Constants.GetAppDataDirectory(), string.Format(Constants.CommandLogFileNameTemplate, Guid.NewGuid()));
        StreamWriter? logWriter;
        try
        {
            logWriter = new StreamWriter(File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true
            };
            Console.SetOut(logWriter);
            Console.SetError(logWriter);
        }
        catch (Exception ex)
        {
            logWriter = null;
            Console.WriteLine("Could not redirect stdout. Error below:");
            Console.WriteLine(ex.ToString());

            // do not delete log in case we could not open the file as it's used by someone else
            await DisposeLogWriterAsync(logWriter, deleteLog: false);
        }

        return logWriter;
    }

    private static async Task DisposeLogWriterAsync(StreamWriter? writer, bool deleteLog)
    {
        if (writer is null)
            return;

        await writer.DisposeAsync();
        if (deleteLog)
        {
            try
            {
                File.Delete(((FileStream)writer.BaseStream).Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not delete log file. Error below:");
                Console.WriteLine(ex.ToString());
            }
        }
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