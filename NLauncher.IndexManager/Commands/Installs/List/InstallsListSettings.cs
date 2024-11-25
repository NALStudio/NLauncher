using NLauncher.IndexManager.Commands.Main;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Installs.List;
internal class InstallsListSettings : MainSettings
{
    [CommandOption("--app <id>")]
    public Guid? AppId { get; set; }

    [CommandOption("-a|--all")]
    public bool ShowAllVersions { get; set; }
}
