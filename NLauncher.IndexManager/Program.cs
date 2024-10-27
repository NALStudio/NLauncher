using NLauncher.IndexManager.Subprograms;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Immutable;
using static NLauncher.IndexManager.MainCommand;

namespace NLauncher.IndexManager;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        CommandApp<MainCommand> app = new();

        app.Configure(config =>
        {
            config.PropagateExceptions();
        });

        try
        {
            return await app.RunAsync(args);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return -99;
        }
    }
}

internal sealed class MainCommand : AsyncCommand<Settings>
{
    private readonly record struct KnownSubprogram(string Name, Subprogram Instance);

    private static readonly ImmutableArray<KnownSubprogram> IndexExistsSubprograms = [
        new("Delete Index", new DeleteIndex())
    ];
    private static readonly ImmutableArray<KnownSubprogram> IndexDoesNotExistSubprograms = [
        new("Create Index", new CreateIndex())
    ];

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[path]")]
        public string? Path { get; init; } = null;
    }

    string? resolvedDirectory = null;
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(resolvedDirectory))
            throw new InvalidOperationException("Validate not called or validate failed.");

        SubprogramContext ctx = new(resolvedDirectory);

        KnownSubprogram subprogram = AnsiConsole.Prompt(
            new SelectionPrompt<KnownSubprogram>()
                .Title("Choose Action")
                .AddChoices(ctx.IndexExists ? IndexExistsSubprograms : IndexDoesNotExistSubprograms)
                .UseConverter(static known => known.Name.EscapeMarkup())
        );

        SubprogramError? error = await subprogram.Instance.MainAsync(ctx);
        if (error is not null)
        {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine(error.Message, new Style(Color.Red));
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        string path = settings.Path ?? Environment.CurrentDirectory;

        resolvedDirectory = TryResolveDictionary(path);
        if (resolvedDirectory is null)
            return ValidationResult.Error($"Invalid index location: '{path}'");

        return ValidationResult.Success();
    }

    private static string? TryResolveDictionary(string path)
    {
        string? filename = Path.GetFileName(path);
        if (string.Equals(filename, Constants.IndexMetaFilename, StringComparison.OrdinalIgnoreCase))
        {
            // Path points to a file
            return Path.GetDirectoryName(path);
        }
        else
        {
            // Path points to a dictionary
            return path;
        }
    }
}