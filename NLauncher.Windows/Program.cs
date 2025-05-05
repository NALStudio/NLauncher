using NLauncher.Windows.Commands.Default.Install;
using NLauncher.Windows.Commands.Default.Run;
using NLauncher.Windows.Commands.Default.Uninstall;
using NLauncher.Windows.Commands.Protoc;
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
                return RunConsole(argsRest);
            case "protoc":
                return RunUrlProtocol(argsRest);
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

    private static void RunWinForms(string[] args)
    {
        // We only do very limited input handling so that our URL handling won't be an attack vector to the app
        string? path = null;
        if (args.Length > 1 && args[0] == "--page")
            path = EnsureSafePagePath(args[1]);

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainPage(path));
    }

    /// <summary>
    /// This method blocks the thread until all tasks are complete.
    /// </summary>
    private static int RunConsole(IEnumerable<string> args)
    {
        StreamWriter? logWriter = RedirectStdOutToLog(debugOnly: true);

        bool deleteLogFile = true;
        try
        {
            Console.WriteLine($"Running command: '{Environment.CommandLine}'");
            return BuildDefaultCommandApp().Run(args);
        }
        catch
        {
            deleteLogFile = false;
            throw;
        }
        finally
        {
            DisposeLogWriter(logWriter, deleteLogFile);
        }
    }

    private static int RunUrlProtocol(string[] args)
    {
        StreamWriter? logWriter = RedirectStdOutToLog(debugOnly: true);

        ProtocError? error = null;
        try
        {
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
                    ProtocError? err = command?.ExecuteAsync(cmdArgs).Result;
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
        finally
        {
            DisposeLogWriter(logWriter, deleteLog: error is null);
        }

        return error is null ? 0 : 1;
    }

    public static CommandApp BuildDefaultCommandApp()
    {
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

        return app;
    }

    private static StreamWriter? RedirectStdOutToLog(bool debugOnly)
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
        }
        catch (Exception ex)
        {
            logWriter = null;
            Console.WriteLine("Could not redirect stdout. Error below:");
            Console.WriteLine(ex.ToString());
        }

        return logWriter;
    }

    private static void DisposeLogWriter(StreamWriter? writer, bool deleteLog)
    {
        if (writer is null)
            return;

        writer.Dispose();
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