using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Main;
internal sealed class MainCommand : AsyncCommand<MainSettings>
{
    private readonly record struct KnownCommand(string Name, ICommand<MainSettings> Instance);

    private static readonly ImmutableArray<KnownCommand> IndexExistsSubprograms = [
        new("Add Application", new AddCommand()),
        new("Delete Index", new DeleteCommand())
    ];
    private static readonly ImmutableArray<KnownCommand> IndexDoesNotExistSubprograms = [
        new("Create Index", new CreateCommand())
    ];

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings)
    {
        KnownCommand command = AnsiConsole.Prompt(
            new SelectionPrompt<KnownCommand>()
                .Title("Choose Action")
                .AddChoices(settings.Context.IndexExists ? IndexExistsSubprograms : IndexDoesNotExistSubprograms)
                .UseConverter(static known => known.Name.EscapeMarkup())
        );

        return await command.Instance.Execute(context, settings);
    }
}
