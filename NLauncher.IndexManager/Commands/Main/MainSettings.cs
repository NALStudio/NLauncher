using NLauncher.IndexManager.Components;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Main;
internal class MainSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    public string Path { get; init; } = Environment.CurrentDirectory;

    private IndexContext? ctx = null;
    public IndexContext Context => ctx ?? throw new InvalidOperationException("Validate not called or validate failed.");

    public override ValidationResult Validate()
    {
        ctx = IndexContext.TryResolve(Path);
        if (ctx is null)
            return ValidationResult.Error($"Invalid index location: '{Path}'");
        else
            return ValidationResult.Success();
    }
}