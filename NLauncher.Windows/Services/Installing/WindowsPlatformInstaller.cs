using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLauncher.Index.Enums;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.Index;
using NLauncher.Services.Apps.Installing;
using System.Collections.Frozen;

namespace NLauncher.Windows.Services.Installing;
public class WindowsPlatformInstaller : IPlatformInstaller
{
    private FrozenSet<char>? _invalidFilenameChars;
    public FrozenSet<char> InvalidFilenameChars => _invalidFilenameChars ??= Path.GetInvalidFileNameChars().ToFrozenSet();

    private readonly IServiceProvider serviceProvider;
    public WindowsPlatformInstaller(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public bool InstallSupported(AppInstall install)
    {
        return install is BinaryAppInstall bai && bai.Platform.HasFlag(Platforms.Windows);
    }

    public InstallTask Install(Guid appId, AppInstall install)
    {
        return new WindowsBinaryInstallTask(
            serviceProvider.GetRequiredService<ILogger<WindowsBinaryInstallTask>>(),
            appId,
            (BinaryAppInstall)install,
            uninstall: false
        );
    }

    public InstallTask Uninstall(Guid appId, AppInstall existingInstall)
    {
        return new WindowsBinaryInstallTask(
            serviceProvider.GetRequiredService<ILogger<WindowsBinaryInstallTask>>(),
            appId,
            (BinaryAppInstall)existingInstall,
            uninstall: true
        );
    }

    public ValueTask<bool> IsInstallFound(Guid appId, AppInstall install) => ValueTask.FromResult(InstallExists(appId, install));

    public static bool InstallExists(Guid appId, AppInstall install)
    {
        if (install is not BinaryAppInstall bai)
            return false;

        string dir = SystemDirectories.GetLibraryPath(appId).FullName;
        return File.Exists(Path.Join(dir, bai.ExecutablePath));
    }

    public bool ShortcutSupported(AppInstall install) => install is BinaryAppInstall;

    public async ValueTask CreateShortcut(IndexEntry app, AppInstall install)
    {
        if (install is not BinaryAppInstall bai)
            throw new ArgumentException($"Install not supported: '{install.GetType().Name}'");

        string? filename = TryGetFilename(app.Manifest.DisplayName);
        filename ??= TryGetFilename(app.Manifest.Uuid.ToString());
        filename ??= "NLauncher Shortcut";

        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string filepath = EnsureUniqueFilepath(Path.Join(desktop, filename + ".url"));

        await using FileStream fs = File.Open(filepath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await using StreamWriter writer = new(fs);

        Guid appId = app.Manifest.Uuid;
        string exePath = Path.Join(SystemDirectories.GetLibraryPath(appId).FullName, bai.ExecutablePath);

        // https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop/4897700#4897700
        await writer.WriteLineAsync("[InternetShortcut]");

        await writer.WriteAsync("URL=nlauncher://rungameid/");
        await writer.WriteLineAsync(appId.ToString());

        await writer.WriteLineAsync("IconIndex=0");

        await writer.WriteAsync("IconFile=");
        await writer.WriteLineAsync(exePath.Replace('\\', '/'));
    }

    private static string EnsureUniqueFilepath(string filepath)
    {
        if (!Path.Exists(filepath))
            return filepath;

        ReadOnlySpan<char> name, ext;
        int periodIndex = filepath.LastIndexOf('.');
        if (periodIndex == -1)
        {
            name = filepath;
            ext = ReadOnlySpan<char>.Empty;
        }
        else
        {
            name = filepath.AsSpan(0, periodIndex);
            ext = filepath.AsSpan(periodIndex);
        }

        int i = 0;
        do
        {
            i++;
            filepath = $"{name} ({i}){ext}";
        } while (Path.Exists(filepath));

        return filepath;
    }

    private string? TryGetFilename(string displayName)
    {
        int filenameLength = displayName.Length;
        if (filenameLength > 32) // limit length due to stackalloc
            filenameLength = 32;

        int len = 0;
        Span<char> fn = stackalloc char[filenameLength];
        for (int i = 0; i < displayName.Length; i++)
        {
            if (len >= filenameLength)
                break;

            char c = displayName[i];
            if (!InvalidFilenameChars.Contains(c))
            {
                fn[len] = c;
                len++;
            }
        }

        if (len > 0)
            return new string(fn[..len]);
        else
            return null;
    }
}
