using NLauncher.IndexManager.Commands.Main;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Installs;
internal class InstallsCreateCommand : AsyncCommand<MainSettings>, IMainCommand
{
    // TODO: Create install
    // allow user to pick the version or create a new one and then create the install there
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);

    public Task<int> ExecuteAsync(MainSettings settings)
    {
        throw new NotImplementedException();
    }

    public ValidationResult Validate(MainSettings settings)
    {
        throw new NotImplementedException();
    }
}
