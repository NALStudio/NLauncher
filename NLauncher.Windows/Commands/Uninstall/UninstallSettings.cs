using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Uninstall;

public class UninstallSettings : CommandSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public Guid AppId { get; set; }
}
