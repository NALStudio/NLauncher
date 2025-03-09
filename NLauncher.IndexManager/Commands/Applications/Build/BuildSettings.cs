using NLauncher.Index.Enums;
using NLauncher.IndexManager.Commands.Main;
using Spectre.Console.Cli;

namespace NLauncher.IndexManager.Commands.Applications.Build;

internal interface IBuildSettings
{
    public string? OutputPath { get; }
    public bool MinifyOutput { get; }
    public IndexEnvironment? Environment { get; }
}

internal class BuildSettings : MainSettings, IBuildSettings
{
    [CommandOption("-o|--output <path>")]
    public string? OutputPath { get; set; }

    [CommandOption("-m|--minify")]
    public bool MinifyOutput { get; set; } = false;

    [CommandOption("-e|--environment <env>")]
    public IndexEnvironment? Environment { get; set; }
}
