using NLauncher.IndexManager.Commands.Commands;
using NLauncher.IndexManager.Commands.Commands.Aliases;
using NLauncher.IndexManager.Commands.Commands.Build;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Commands.Commands.Main;
internal sealed class MainCommand : AsyncCommand<MainSettings>
{
    private readonly struct KnownCommand
    {
        public string Name { get; }

        public Func<IMainCommand>? Instantiate { get; }
        public ImmutableArray<KnownCommand> Children { get; }

        private KnownCommand(string name, Func<IMainCommand>? instantiate, ImmutableArray<KnownCommand> children)
        {
            Name = name;
            Instantiate = instantiate;
            Children = children;
        }

        public static KnownCommand Command<T>(string name) where T : ICommand, IMainCommand, new()
        {
            return new KnownCommand(
                name: name,
                instantiate: static () => new T(),
                children: ImmutableArray<KnownCommand>.Empty
            );
        }

        public static KnownCommand Category(string name, ImmutableArray<KnownCommand> childCommands)
        {
            return new KnownCommand(
                name: name,
                instantiate: null,
                children: childCommands
            );
        }
    }

    private static readonly KnownCommand IndexExistsCommandTree = KnownCommand.Category(
        "Root",
        [
            KnownCommand.Command<AddCommand>("Add Application"),
            KnownCommand.Category(
                "URL Aliases",
                [
                    KnownCommand.Command<AliasesAddCommand>("Add Alias"),
                    KnownCommand.Command<AliasesListCommand>("List All"),
                ]
            ),
            KnownCommand.Command<BuildCommand>("Build Index"),
            KnownCommand.Command<DeleteCommand>("Delete Index"),
        ]
    );

    private static readonly KnownCommand IndexDoesNotExistCommandTree = KnownCommand.Category(
        "Root",
        [
            KnownCommand.Command<CreateCommand>("Create Index")
        ]
    );

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings)
    {
        KnownCommand command = settings.Context.IndexExists ? IndexExistsCommandTree : IndexDoesNotExistCommandTree;
        IMainCommand? commandInstance = null;
        while (commandInstance is null)
        {
            command = AnsiConsole.Prompt(ConstructSelect(command));
            commandInstance = command.Instantiate?.Invoke();
        }

        ValidationResult validation = commandInstance.Validate(settings);
        if (!validation.Successful)
        {
            AnsiConsole.MarkupLine($"[red]{validation.Message.EscapeMarkup()}[/]");
            return -99;
        }

        return await commandInstance.ExecuteAsync(settings);
    }

    private static SelectionPrompt<KnownCommand> ConstructSelect(KnownCommand root)
    {
        SelectionPrompt<KnownCommand> selection = new SelectionPrompt<KnownCommand>()
            .Title("Choose Action")
            .UseConverter(static cmd => cmd.Name);

        // Construct command tree
        foreach (KnownCommand command in root.Children)
        {
            ISelectionItem<KnownCommand> choice = selection.AddChoice(command);
            AddChildren(choice, command);
        }

        return selection;
    }

    private static void AddChildren(ISelectionItem<KnownCommand> choice, KnownCommand command)
    {
        foreach (KnownCommand childCommand in command.Children)
        {
            ISelectionItem<KnownCommand> childItem = choice.AddChild(childCommand);
            AddChildren(childItem, childCommand);
        }
    }
}
