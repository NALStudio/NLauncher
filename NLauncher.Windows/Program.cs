using NLauncher.Windows.Run;

namespace NLauncher.Windows;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static async Task Main(string[] args)
    {
        bool wasRun = await TryRunFromArgs(args);

        if (!wasRun)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainPage());
        }
    }

    private static async ValueTask<bool> TryRunFromArgs(string[] args)
    {
        if (args.Length < 1)
            return false;

        string cmd = args[0];
        string[] argsLeft = args[1..];
        return cmd switch
        {
            "rungameid" => await TryRunApp(argsLeft),
            "install" => await TryRunInstall(argsLeft),
            _ => false,
        };
    }

    private static async ValueTask<bool> TryRunInstall(string[] args)
    {

    }

    private static async ValueTask<bool> TryRunApp(string[] args)
    {
        if (args.Length < 1)
            return false;

        if (!Guid.TryParse(args[0], out Guid appId))
            return false;

        try
        {
            await RunApp.Run(appId);
        }
        catch (Exception e)
        {
            // TODO: Handle error
            throw;
        }

        return true;
    }
}