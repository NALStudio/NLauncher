using NLauncher.Windows.Commands.Install;
using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands;

internal class RunSettings : CommandSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public required Guid AppId { get; init; }
}

internal class RunCommand : AsyncCommand<InstallSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, InstallSettings settings)
    {
        throw new NotImplementedException();
    }
}
