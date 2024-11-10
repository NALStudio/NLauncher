using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;

namespace NLauncher.IndexManager.Commands;
internal partial class AliasesAddCommand : AsyncCommand<MainSettings>, IMainCommand
{
    private readonly record struct AliasAddResult
    {
        public string? ErrorMessage { get; }
        public string? WarningMessage { get; }

        private AliasAddResult(string? error, string? warning)
        {
            ErrorMessage = error;
            WarningMessage = warning;
        }

        public static AliasAddResult Success() => new(null, null);
        public static AliasAddResult Warning(string message) => new(error: null, warning: message);
        public static AliasAddResult Error(string message) => new(error: message, warning: null);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        IndexPaths paths = settings.Context.Paths;

        AnsiFormatter.WriteHeader("New Application Alias");

        ReadOnlyCollection<AppManifest> manifests = await AnsiConsole.Status().StartAsync(
            "Loading index applications...",
            async _ => await LoadIndexApplications(paths)
        );

        AnsiFormatter.WriteSectionTitle("Create Alias");

        AppManifest manifest = AnsiConsole.Prompt(
            new SelectionPrompt<AppManifest>()
                .Title("Choose Application")
                .EnableSearch()
                .AddChoices(manifests)
                .UseConverter(static manifest => manifest.DisplayName)
        );

        string? chooseAliasDefault = DefaultAlias(manifest);
        TextPrompt<string> chooseAliasPrompt = new TextPrompt<string>("Choose Alias").Validate(ValidateAlias);
        if (chooseAliasDefault is not null)
            chooseAliasPrompt.DefaultValue(chooseAliasDefault);
        string alias = AnsiConsole.Prompt(chooseAliasPrompt);

        AnsiConsole.WriteLine("Adding Alias...");
        AnsiConsole.WriteLine();

        AliasAddResult result;
        using (FileChangeTree.ListenAndWrite(paths.Directory))
        {
            result = await AddAlias(paths, manifest, alias);
        }

        if (result.ErrorMessage is string error)
        {
            AnsiConsole.MarkupLine($"[red]{error.EscapeMarkup()}[/]");
            return 1;
        }
        else
        {
            if (result.WarningMessage is string warning)
                AnsiConsole.MarkupLine($"[yellow]{warning.EscapeMarkup()}[/]");
            return 0;
        }
    }

    private static async Task<AliasAddResult> AddAlias(IndexPaths paths, AppManifest manifest, string alias)
    {
        string oldAliasesJson = await File.ReadAllTextAsync(paths.AliasesFile);
        AppAliases? oldAliases = IndexJsonSerializer.Deserialize<AppAliases>(oldAliasesJson);
        if (oldAliases is null)
            return AliasAddResult.Error("Could not deserialize app aliases.");

        if (oldAliases.Aliases.TryGetValue(alias, out Guid value))
        {
            if (value == manifest.Uuid)
                return AliasAddResult.Warning("Alias already registered for this application.");
            else
                return AliasAddResult.Error("Alias is already registered for another application.");
        }

        AppAliases newAliases = new(oldAliases.Aliases.Add(alias, manifest.Uuid));
        string newAliasesJson = IndexJsonSerializer.Serialize(newAliases, IndexSerializationOptions.HumanReadable);
        await File.WriteAllTextAsync(paths.AliasesFile, newAliasesJson);

        return AliasAddResult.Success();
    }

    private static async Task<ReadOnlyCollection<AppManifest>> LoadIndexApplications(IndexPaths paths)
    {
        List<AppManifest> manifests = new();

        await foreach (AppManifest manifest in ManifestHelper.DiscoverManifestsAsync(paths))
            manifests.Add(manifest);

        return manifests.AsReadOnly();
    }

    private string? DefaultAlias(AppManifest manifest)
    {
        string lowered = manifest.DisplayName.ToLowerInvariant();
        string alias = WhitespaceRegex().Replace(lowered, "-");

        if (ValidateAlias(alias).Successful)
            return alias;
        else
            return null;
    }
    private ValidationResult ValidateAlias(string alias)
    {
        if (!StringIsLowercaseAscii(alias.AsSpan()))
            return ValidationResult.Error("Alias must be lowercase ASCII.");
        if (WhitespaceRegex().IsMatch(alias))
            return ValidationResult.Error("Alias cannot contain whitespace.");

        char[] notUrlEncodable = NotUrlEncodableCharacters(alias).ToArray();
        if (notUrlEncodable.Length > 0)
            return ValidationResult.Error($"Alias must be URL encodable. Offending characters: {string.Join(' ', notUrlEncodable)}");

        return ValidationResult.Success();
    }

    private static bool StringIsLowercaseAscii(ReadOnlySpan<char> str)
    {
        foreach (char c in str)
        {
            // Only if character is letter, check that it is lower
            // so that we allow for special characters like '-' that can be URL encoded.
            if (char.IsAsciiLetter(c) && !char.IsAsciiLetterLower(c))
                return false;
        }

        return true;
    }

    private static IEnumerable<char> NotUrlEncodableCharacters(string alias)
    {
        foreach (char c in alias)
        {
            string charString = char.ToString(c);
            if (charString != WebUtility.UrlEncode(charString))
                yield return c;
        }
    }

    public ValidationResult Validate(MainSettings settings)
    {
        return ValidationResult.Success();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
