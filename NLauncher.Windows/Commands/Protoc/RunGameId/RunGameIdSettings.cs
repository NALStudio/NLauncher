using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Protoc.RunGameId;
internal class RunGameIdSettings : CommandSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public Guid AppId { get; set; }
}
