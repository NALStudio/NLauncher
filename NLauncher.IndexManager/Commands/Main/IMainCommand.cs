using Spectre.Console;

namespace NLauncher.IndexManager.Commands.Commands.Main;
internal interface IMainCommand
{
    public Task<int> ExecuteAsync(MainSettings settings);
    public ValidationResult Validate(MainSettings settings);
}
