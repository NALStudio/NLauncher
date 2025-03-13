using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Install;
public class InstallSettings : BaseSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public Guid AppId { get; set; }
}