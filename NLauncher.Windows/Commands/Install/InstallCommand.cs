using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Index.Models.InstallTracking;
using NLauncher.Services.Index;
using NLauncher.Web.Services;
using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Install;

internal class InstallSettings : CommandSettings
{
    [CommandArgument(0, "<INSTALL_ID>")]
    public required string InstallId { get; init; }
}

internal class InstallCommand : AsyncCommand<InstallSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, InstallSettings settings)
    {
        if (!InstallGuid.TryParse(settings.InstallId, out InstallGuid installId))
        {
            Console.WriteLine("Invalid install id.");
            return 1;
        }

        IndexService indexService = new(null, Program.HttpClient, new WindowsStorageService());
        IndexManifest index = await indexService.GetIndexAsync();

        if (!index.TryFindInstall(installId, out AppInstall? install))
        {
            Console.WriteLine("Install not found.");
            return 1;
        }

        return install switch
        {
            BinaryAppInstall bai => await HandleBinaryInstall.InstallBinaryAsync(installId, bai),
            _ => InvalidInstallType(install)
        };
    }

    private static int InvalidInstallType(AppInstall install)
    {
        Console.WriteLine($"Invalid install type: '{install.GetType().Name}'. Not supported.");
        return 1;
    }
}
