using NLauncher.Windows.Commands.Default;
using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Default.Install;
public class InstallSettings : DefaultSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public Guid AppId { get; set; }
}