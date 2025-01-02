using NLauncher.Index.Enums;
using NLauncher.Index.Json;
using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.IndexManager.Commands.Installs.Add.BinaryProviders;
using NLauncher.IndexManager.Commands.Main;
using NLauncher.IndexManager.Components;
using NLauncher.IndexManager.Components.FileChangeTree;
using NLauncher.IndexManager.Components.Paths;
using NLauncher.IndexManager.Components.PromptUtils;
using NLauncher.IndexManager.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;

namespace NLauncher.IndexManager.Commands.Installs.Add;
internal class InstallsAddCommand : AsyncCommand<MainSettings>, IMainCommand
{
    private readonly record struct SelectedVersion(AppVersion? Version);

    public override async Task<int> ExecuteAsync(CommandContext context, MainSettings settings) => await ExecuteAsync(settings);
    public override ValidationResult Validate(CommandContext context, MainSettings settings) => Validate(settings);
    public ValidationResult Validate(MainSettings settings) => ValidationResult.Success();

    private static readonly FrozenDictionary<string, Func<Task<AppInstall?>>> InstallBuilders = new Dictionary<string, Func<Task<AppInstall?>>>()
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

        var typePrompt = new SelectionPrompt<KeyValuePair<string, Func<Task<AppInstall?>>>>()
            .Title("Select Install Type")
            .AddChoices(InstallBuilders)
            .UseConverter(static kv => kv.Key);

        Func<Task<AppInstall?>> constructFunc = AnsiConsole.Prompt(typePrompt).Value;

        AppInstall? install = await constructFunc();
        if (install is null)
            return 1;

        AnsiFormatter.WriteSectionTitle("Create Log");

        OperationResult result;
        using (FileChangeTree.ListenAndWrite(paths.Directory))
        {
            ManifestPaths manifestPaths = new(manifest.DirectoryPath);
            result = await AddInstall(manifestPaths, verNum, createVersion, install);
            await Task.Delay(100); // Fix race condition (file change is invoked after FileChangeTree is disposed.
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
            .Validate(v => v >= minVersion, $"Version number must be at least: {minVersion}");
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
        AppManifest? manifest = JsonSerializer.Deserialize<AppManifest>(oldManifestJson, IndexJsonContext.Default.AppManifest);
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

        string newManifestJson = JsonSerializer.Serialize(manifest, IndexJsonContext.HumanReadable.AppManifest);
        await File.WriteAllTextAsync(paths.ManifestFile, newManifestJson);

        return OperationResult.Success();
    }

    private static async Task<AppInstall?> ConstructBinary()
    {
        Platforms platform = AnsiConsole.Prompt(
            new SelectionPrompt<Platforms>()
                .Title("Choose Supported Platform")
                .AddChoices(Enum.GetValues<Platforms>().Where(static p => p != Platforms.None))
        );

        InstallBinaryProvider provider = AnsiConsole.Prompt(
            new SelectionPrompt<InstallBinaryProvider>()
                .Title("Choose Binary Source")
                .AddChoices(new WebFileInstallBinaryProvider())
                .UseConverter(static p => p.DisplayName)
        );

        AnsiFormatter.WriteSectionTitle("Verify Files");
        InstallBinaryProvider.FileData? fileData = await provider.LoadFileDataAsync();
        if (fileData is null)
            return null;

        string? executablePath = TrySelectExecutable(fileData.Files);
        if (executablePath is null)
            return null;
        if (Path.IsPathRooted(executablePath)) // Path is not relative if it's rooted
        {
            AnsiFormatter.WriteError("Path must be relative.");
            return null;
        }

        return new BinaryAppInstall()
        {
            Platform = platform,
            ExecutablePath = executablePath,

            DownloadUrl = fileData.DownloadUrl,
            DownloadHash = Convert.ToBase64String(fileData.Hash.AsSpan())
        };
    }

    private static string? TrySelectExecutable(ImmutableArray<string> files)
    {
        string[] executables = files.Where(static f => Path.GetExtension(f) == ".exe").ToArray();
        if (executables.Length < 1)
        {
            AnsiFormatter.WriteError("No executables found.");
            return null;
        }

        string? commonRootFolder = GetCommonRootFolder(files);
        if (commonRootFolder is not null)
        {
            for (int i = 0; i < executables.Length; i++)
                executables[i] = Path.GetRelativePath(commonRootFolder, executables[i]);
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose Executable")
                .AddChoices(executables)
        );
    }

    private static string? GetRootFolder(string? path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir)) // If file does not have a parent directory, it also doesn't have a root folder.
            return null;

        // Find the last directory. Taking the directory name of this directory results in null.
        while (true)
        {
            string? newDir = Path.GetDirectoryName(dir);

            if (string.IsNullOrEmpty(newDir))
                return dir;

            dir = newDir;
        }
    }

    private static string? GetCommonRootFolder(IList<string> files)
    {
        if (files.Count < 1)
            return null;

        string? rootFolder = GetRootFolder(files[0]);
        if (rootFolder is null)
            return null;

        if (files.All(f => f.StartsWith(rootFolder)))
            return rootFolder;
        else
            return null;
    }

    private static Task<AppInstall?> ConstructStoreLink()
    {
        Platforms platform = AnsiConsole.Prompt(
            new SelectionPrompt<Platforms>()
                .Title("Store platform:")
                .AddChoices(Enum.GetValues<Platforms>().Where(static p => p != Platforms.None))
        );

        Uri url = AnsiConsole.Ask<Uri>("Store link:");

        StoreLinkAppInstall install = new()
        {
            Platform = platform,
            Url = url
        };

        return Task.FromResult<AppInstall?>(install);
    }

    private static Task<AppInstall?> ConstructWebsite()
    {
        Uri url = AskUri("Website link:");
        bool supportsPwa = AnsiConsole.Confirm("Does your website support PWA?", defaultValue: false);

        WebsiteAppInstall install = new()
        {
            Url = url,
            SupportsPwa = supportsPwa
        };

        return Task.FromResult<AppInstall?>(install);
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
