using NLauncher.IndexManager.Commands.Build;
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
    private readonly record struct KnownCommand(string Name, Func<CommandContext, MainSettings, Task<int>> ExecuteAsync);

    private static readonly ImmutableArray<KnownCommand> IndexExistsSubprograms = [
        new("Add Application", (ctx, s) => new AddCommand().ExecuteAsync(ctx, s)),
        new("Build Index", (ctx, s) => new BuildCommand().ExecuteAsync(ctx, s, outputPath: null, minifyOutput: false)),
        new("Build Index (minify)", (ctx, s) => new BuildCommand().ExecuteAsync(ctx, s, outputPath: null, minifyOutput: true)),
        new("Delete Index", (ctx, s) => Task.FromResult(new DeleteCommand().Execute(ctx, s)))
    ];
    private static readonly ImmutableArray<KnownCommand> IndexDoesNotExistSubprograms = [
        new("Create Index", (ctx, s) => new CreateCommand().ExecuteAsync(ctx, s))
    ];

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings)
    {
        KnownCommand command = AnsiConsole.Prompt(
            new SelectionPrompt<KnownCommand>()
                .Title("Choose Action")
                .AddChoices(settings.Context.IndexExists ? IndexExistsSubprograms : IndexDoesNotExistSubprograms)
                .UseConverter(static known => known.Name.EscapeMarkup())
        );

        return await command.ExecuteAsync(context, settings);
    }
}
