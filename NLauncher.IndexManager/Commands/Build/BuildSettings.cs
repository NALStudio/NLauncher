using NLauncher.IndexManager.Commands.Main;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Build;
internal class BuildSettings : MainSettings
{
    [CommandOption("-o|--output <path>")]
    public string? OutputPath { get; set; }

    [CommandOption("-m|--minify")]
    public bool MinifyOutput { get; set; } = false;
}
