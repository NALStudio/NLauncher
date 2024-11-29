using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Services.Storage;
public class WindowsStorageService : IStorageService
{
    private const string appDataDirName = "NLauncher";

    public async ValueTask<string> ReadAll(string filename)
    {
        string filepath = GetFilePath(filename, createDir: false);
        if (File.Exists(filepath))
            return await File.ReadAllTextAsync(filepath);
        else
            return string.Empty;
    }

    public async ValueTask WriteAll(string filename, string text)
    {
        string filepath = GetFilePath(filename, createDir: true);
        await File.WriteAllTextAsync(filepath, text);
    }

    /// <summary>
    /// Get the file path of the given <paramref name="filename"/>. Creates the AppData directory if it doesn't yet exist and <paramref name="create"/> is set as <see langword="true"/>
    /// </summary>
    private static string GetFilePath(string filename, bool createDir)
    {
        IStorageService.ThrowIfFilenameInvalid(filename);

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        appData = Path.Join(appData, appDataDirName);

        if (createDir && !Directory.Exists(appData))
            Directory.CreateDirectory(appData);

        return Path.Join(appData, filename);
    }
}
