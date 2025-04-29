using NLauncher.Windows.Models;
using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Default.Uninstall;

internal class BinaryUninstallCommand : AsyncCommand<UninstallSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, UninstallSettings settings)
    {
        await using CommandOutput output = await settings.ConnectOutputAsync();
        output.WriteLine("Uninstalling...");

        DirectoryInfo dir = SystemDirectories.GetLibraryPath(settings.AppId);
        if (dir.Exists)
            dir.Delete(recursive: true);

        return 0;
    }
}
