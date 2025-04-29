using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Default.Uninstall;

public class UninstallSettings : DefaultSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public Guid AppId { get; set; }
}
