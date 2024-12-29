using NLauncher.Services;

namespace NLauncher.Windows.Services;

public class WindowsStorageService : IStorageService
{
    public static string AppDataPath { get; } = GetAppDataPath();

    private static string GetAppDataPath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Join(appData, Constants.AppDataDirname);
    }

    private static string GetPath(string filename)
    {
        IStorageService.ThrowIfFilenameInvalid(filename);
        return Path.Join(AppDataPath, filename);
    }

    public async ValueTask WriteAll(string filename, string text)
    {
        string path = GetPath(filename);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text);
    }

    public async ValueTask<string> ReadAll(string filename)
    {
        string path = GetPath(filename);
        if (File.Exists(path))
            return await File.ReadAllTextAsync(path);
        else
            return string.Empty;
    }
}
