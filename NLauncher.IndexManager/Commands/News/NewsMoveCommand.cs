using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components.FileChangeTree;
using NLauncher.IndexManager.Components.Paths;
using NLauncher.IndexManager.Components.PromptUtils;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.News;
internal class NewsMoveCommand : AsyncCommand<MainSettings>, IMainCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
    public ValidationResult Validate(MainSettings settings) => ValidationResult.Success();

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        IndexPaths paths = settings.Context.Paths;

        NewsPaths? moveSource = await NewsSlotPrompt.AskNewsPath(paths, allowEmpty: false);
        if (moveSource is null)
        {
            NewsSlotPrompt.PrintNoSlotsAvailable();
            return 1;
        }

        NewsPaths? moveDest = await NewsSlotPrompt.AskNewsPath(paths, title: "Choose Destination Slot", allowReserved: false);
        if (moveDest is null)
        {
            NewsSlotPrompt.PrintNoSlotsAvailable();
            return 1;
        }

        using (FileChangeTree.ListenAndWrite(paths.Directory))
        {
            AnsiConsole.Status().Start(
                "Moving news...",
                _ => Directory.Move(moveSource.Directory, moveDest.Directory)
            );
        }

        return 0;
    }
}
