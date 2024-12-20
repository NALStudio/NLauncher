using Downloader;
using NLauncher.Code;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Index.Models.InstallTracking;
using System.IO.Compression;
using System.Security.Cryptography;

namespace NLauncher.Windows.Commands.Install;
internal static class HandleBinaryInstall
{
    public static async Task<int> InstallBinaryAsync(InstallGuid installId, BinaryAppInstall install)
    {
        DirectoryInfo downloadDir = SystemDirectories.GetDownloadsPath(installId.AppId);
        string downloadPath = Path.Join(downloadDir.FullName, ".download");

        Console.WriteLine("Downloading...");
        await DownloadBinary(install.DownloadUrl, downloadPath);

        Console.WriteLine("Verifying...");
        if (!await VerifyHash(downloadPath, install.DownloadHash))
        {
            Console.WriteLine("Downloaded file hash does not match the expected hash.");
            return 1;
        }

        Console.WriteLine("Installing...");
        DirectoryInfo gameDir = SystemDirectories.GetLibraryPath(installId.AppId);
        Unzip(downloadPath, gameDir);

        return 0;
    }

    private static async Task DownloadBinary(Uri url, string filepath)
    {
        DownloadService downloadService = new();

        downloadService.DownloadStarted += DownloadService_DownloadStarted;
        downloadService.DownloadProgressChanged += DownloadService_DownloadProgressChanged;

        Console.WriteLine("Downloading...");
        await downloadService.DownloadFileTaskAsync(url.ToString(), filepath);
    }

    private static async Task<bool> VerifyHash(string filepath, string expectedHash)
    {
        byte[] expected = Convert.FromBase64String(expectedHash);
        byte[] downloadHash;
        await using (FileStream fs = File.OpenRead(filepath))
            downloadHash = await SHA256.HashDataAsync(fs);

        return expected.AsSpan().SequenceEqual(downloadHash.AsSpan());
    }

    private static void Unzip(string zipPath, DirectoryInfo gameDir)
    {
        // Delete old game directory if it exists
        if (gameDir.Exists)
            gameDir.Delete(recursive: true);
        gameDir.Create();

        ZipFile.ExtractToDirectory(zipPath, gameDir.FullName);

        FileSystemInfo[] infos = gameDir.GetFileSystemInfos().Take(2).ToArray();
        if (infos.Length == 1 && infos[0] is DirectoryInfo dir) // If extracted archive only contains a single directory, move directory upwards so that the games folder contains the actual gamefiles
            dir.MoveTo(gameDir.FullName);
    }

    private static void DownloadService_DownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        DownloadProgress progress = new(DownloadedBytes: 0, TotalBytes: e.TotalBytesToReceive);
        Console.WriteLine(progress.ToString());
    }

    private static void DownloadService_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        DownloadProgress progress = new(DownloadedBytes: e.ReceivedBytesSize, TotalBytes: e.TotalBytesToReceive);
        Console.WriteLine(progress.ToString());
    }
}
