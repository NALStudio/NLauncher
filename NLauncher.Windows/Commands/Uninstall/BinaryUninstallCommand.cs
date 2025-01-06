using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Uninstall;

internal class BinaryUninstallCommand : Command<UninstallSettings>
{
    public override int Execute(CommandContext context, UninstallSettings settings)
    {
        Console.WriteLine("Uninstalling...");

        DirectoryInfo dir = SystemDirectories.GetLibraryPath(settings.AppId);
        if (dir.Exists)
            dir.Delete(recursive: true);

        return 0;
    }
}
