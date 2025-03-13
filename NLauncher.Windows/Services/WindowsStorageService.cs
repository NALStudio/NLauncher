using NLauncher.Services;
using System.Text;

namespace NLauncher.Windows.Services;

public class WindowsStorageService : IStorageService
{
    public static string AppDataPath { get; } = Constants.GetAppDataDirectory();
    public static string CachePath { get; } = Path.Join(AppDataPath, Constants.CacheDirectory);

    private static string GetPath(string filename)
    {
        IStorageService.ThrowIfFilenameInvalid(filename);
        return Path.Join(AppDataPath, filename);
    }

    private static string GetCachePath(string key)
    {
        IStorageService.ThrowIfFilenameInvalid(key, requireExtension: false);
        return Path.Join(CachePath, key + ".cache");
    }

    public async ValueTask Write(string filename, byte[] utf8) => await InternalWriteUtf8(AppDataPath, GetPath(filename), utf8);
    public async ValueTask Write(string filename, string value) => await InternalWriteText(AppDataPath, GetPath(filename), value);
    public async ValueTask WriteCache(string key, byte[] utf8) => await InternalWriteUtf8(CachePath, GetCachePath(key), utf8);

    public async ValueTask<byte[]> ReadUtf8(string filename) => await InternalReadUtf8(GetPath(filename));
    public async ValueTask<string> Read(string filename) => await InternalReadText(GetPath(filename));
    public async ValueTask<byte[]> ReadCache(string key) => await InternalReadUtf8(GetCachePath(key));

    private static async Task InternalWriteUtf8(string parentDir, string fullPath, byte[] bytes)
    {
        Directory.CreateDirectory(parentDir);
        await File.WriteAllBytesAsync(fullPath, bytes);
    }

    private static async Task InternalWriteText(string parentDir, string fullPath, string value)
    {
        Directory.CreateDirectory(parentDir);
        await File.WriteAllTextAsync(fullPath, value, Encoding.UTF8);
    }

    private static async Task<byte[]> InternalReadUtf8(string path)
    {
        if (File.Exists(path))
            return await File.ReadAllBytesAsync(path);
        else
            return Array.Empty<byte>();
    }

    private static async Task<string> InternalReadText(string path)
    {
        if (File.Exists(path))
            return await File.ReadAllTextAsync(path, Encoding.UTF8);
        else
            return string.Empty;
    }
}
