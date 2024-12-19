using Spectre.Console.Cli;

namespace NLauncher.Windows.Commands.Install;

internal class InstallSettings : CommandSettings
{
    [CommandArgument(0, "<APP_ID>")]
    public required Guid AppId { get; init; }

    [CommandOption("-v|--version <VERSION>")]
    public uint? VerNum { get; init; }
}

internal class InstallCommand : AsyncCommand<InstallSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, InstallSettings settings)
    {
        throw new NotImplementedException();
    }
}
