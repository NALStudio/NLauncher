using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Uninstall;

public class UninstallSettings : BaseSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public Guid AppId { get; set; }
}
