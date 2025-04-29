using NLauncher.Windows.Commands.Default;
using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Default.Run;

internal class RunSettings : DefaultSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public required Guid AppId { get; init; }
}
