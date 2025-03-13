using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Run;

internal class RunSettings : BaseSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public required Guid AppId { get; init; }
}
