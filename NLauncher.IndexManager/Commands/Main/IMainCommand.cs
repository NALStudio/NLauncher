using Spectre.Console;

namespace NLauncher.IndexManager.Commands.Main;
internal interface IMainCommand
{
    public Task<int> ExecuteAsync(MainSettings settings);
    public ValidationResult Validate(MainSettings settings);
}

internal interface IMainCommandVariant
{
    public Task<int> ExecuteVariantAsync(MainSettings settings);
}