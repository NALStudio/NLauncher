using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using NLauncher.IndexManager.Components.PromptUtils;
using NLauncher.IndexManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NLauncher.IndexManager.Commands.Installs;
internal class InstallsAddCommand : AsyncCommand<MainSettings>, IMainCommand
{
    private readonly record struct SelectedVersion(AppVersion? Version);

    // TODO: Create install
    // allow user to pick the version or create a new one and then create the install there
    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
    public ValidationResult Validate(MainSettings settings) => ValidationResult.Success();

    private static readonly FrozenDictionary<string, Func<AppInstall?>> InstallBuilders = new Dictionary<string, Func<AppInstall?>>()
    {
        { "Binary", ConstructBinary },
        { "Store Link", ConstructStoreLink },
        { "Website", ConstructWebsite }
    }.ToFrozenDictionary();

    public async Task<int> ExecuteAsync(MainSettings settings)
    {
        IndexPaths paths = settings.Context.Paths;

        AnsiFormatter.WriteHeader("New Application Install");

        DiscoveredManifest manifest = await AppManifestPrompt.AskDiscoveredManifest(paths);

        SelectionPrompt<SelectedVersion> versionSelectPrompt = new SelectionPrompt<SelectedVersion>()
            .Title("Select Version")
            .UseConverter(static sv => sv.Version?.Identifier ?? "Create New")
            .AddChoices(new SelectedVersion(null))
            .AddChoices(manifest.Manifest.Versions.Select(static v => new SelectedVersion(v)));
        SelectedVersion version = AnsiConsole.Prompt(versionSelectPrompt);

        AppVersion? createVersion = null;
        uint verNum;
        if (version.Version is null)
        {
            createVersion = CreateVersion(manifest.Manifest.Versions);
            verNum = createVersion.VerNum;
        }
        else
        {
            verNum = version.Version.VerNum;
        }

        AnsiFormatter.WriteSectionTitle("Create Install");

        var typePrompt = new SelectionPrompt<KeyValuePair<string, Func<AppInstall?>>>()
            .Title("Select Install Type")
            .AddChoices(InstallBuilders)
            .UseConverter(static kv => kv.Key);

        Func<AppInstall?> constructFunc = AnsiConsole.Prompt(typePrompt).Value;
        AppInstall? install = constructFunc();
        if (install is null)
            return 1;

        AnsiConsole.WriteLine("Adding Install...");
        AnsiConsole.WriteLine();

        OperationResult result;
        using (FileChangeTree.ListenAndWrite(paths.Directory))
        {
            ManifestPaths manifestPaths = new(manifest.DirectoryPath);
            result = await AddInstall(manifestPaths, verNum, createVersion, install);
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


    private static AppVersion CreateVersion(ImmutableArray<AppVersion> existingVersions)
    {
        uint minVersion; // minimum version that can be used (inclusive)
        if (existingVersions.Length > 0)
            minVersion = existingVersions.Max(static v => v.VerNum) + 1u;
        else
            minVersion = 0;

        AnsiFormatter.WriteSectionTitle("Create Version");

        TextPrompt<uint> versionPrompt = new TextPrompt<uint>("Version Number:")
            .DefaultValue(minVersion)
            .Validate(v => v < minVersion, $"Version number must be at least: {minVersion}");
        uint verNum = AnsiConsole.Prompt(versionPrompt);

        string verIdentifier = AnsiConsole.Ask("Version Identifier:", defaultValue: "release");

        return new AppVersion()
        {
            VerNum = verNum,
            Identifier = verIdentifier,
            Installs = ImmutableArray<AppInstall>.Empty
        };
    }

    private static async Task<OperationResult> AddInstall(ManifestPaths paths, uint vernum, AppVersion? addVersion, AppInstall install)
    {
        string oldManifestJson = await File.ReadAllTextAsync(paths.ManifestFile);
        AppManifest? manifest = IndexJsonSerializer.Deserialize<AppManifest>(oldManifestJson);
        if (manifest is null)
            return OperationResult.Error("Could not deserialize app manifest.");

        bool findByVernum = false;
        if (addVersion is null)
        {
            addVersion = manifest.Versions.Single(v => v.VerNum == vernum);
            findByVernum = true;
        }

        addVersion = addVersion with
        {
            Installs = addVersion.Installs.Add(install)
        };

        List<AppVersion> versions = manifest.Versions.ToList();
        if (findByVernum)
        {
            int index = versions.FindIndex(v => v.VerNum == addVersion.VerNum);
            if (index == -1)
                throw new Exception("Version not found.");
            versions[index] = addVersion;
        }
        else
        {
            if (versions.Any(v => v.VerNum == addVersion.VerNum))
                throw new Exception("Version exists already.");
            versions.Add(addVersion);
        }

        manifest = manifest with
        {
            Versions = versions.ToImmutableArray()
        };

        string newManifestJson = IndexJsonSerializer.Serialize(manifest, IndexSerializationOptions.HumanReadable);
        await File.WriteAllTextAsync(paths.ManifestFile, newManifestJson);

        return OperationResult.Success();
    }

    private static BinaryAppInstall? ConstructBinary()
    {
        throw new NotImplementedException();
    }

    private static StoreLinkAppInstall? ConstructStoreLink()
    {
        Platforms platform = AnsiConsole.Prompt(
            new SelectionPrompt<Platforms>()
                .Title("Store platform:")
                .AddChoices(Enum.GetValues<Platforms>().Where(static p => p != Platforms.None))
        );

        Uri url = AnsiConsole.Ask<Uri>("Store link:");

        return new StoreLinkAppInstall()
        {
            Platform = platform,
            Url = url
        };
    }

    private static WebsiteAppInstall? ConstructWebsite()
    {
        Uri url = AskUri("Website link:");
        bool supportsPwa = AnsiConsole.Confirm("Does your website support PWA?", defaultValue: false);

        return new WebsiteAppInstall()
        {
            Url = url,
            SupportsPwa = supportsPwa
        };
    }

    private static Uri AskUri(string prompt, Uri? defaultValue = null)
    {
        TextPrompt<Uri> p = new TextPrompt<Uri>(prompt)
            .Validate(ValidateAsHttpsUrl);

        if (defaultValue is not null)
            p.DefaultValue(defaultValue);

        return AnsiConsole.Prompt(p);
    }

    private static ValidationResult ValidateAsHttpsUrl(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            return ValidationResult.Error("URL must be absolute.");

        if (uri.Scheme != Uri.UriSchemeHttps)
            return ValidationResult.Error("URL scheme must be HTTPS.");

        return ValidationResult.Success();
    }
}
