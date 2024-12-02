using NLauncher.Services;
using NLauncher.Windows;

namespace NLauncher.Web.Services;

public partial class WindowsStorageService : IStorageService
{
    private static string? appDataPath;

    private static string GetAppDataPath(string filename)
    {
        IStorageService.ThrowIfFilenameInvalid(filename);

        appDataPath ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Join(appDataPath, Constants.AppDataDirname, filename);
    }

    public async ValueTask WriteAll(string filename, string text)
    {
        string path = GetAppDataPath(filename);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text);
    }

    public async ValueTask<string> ReadAll(string filename)
    {
        string path = GetAppDataPath(filename);
        if (File.Exists(path))
            return await File.ReadAllTextAsync(path);
        else
            return string.Empty;
    }
}
