using Downloader;
using NLauncher.Code;
using NLauncher.Index.Models.Applications.Installs;
using System.Security.Cryptography;

namespace NLauncher.Windows.Commands.Install;
internal class BinaryInstall
{
    private readonly Guid appId;
    private readonly BinaryAppInstall install;

    public BinaryInstall(Guid appId, BinaryAppInstall install)
    {
        this.appId = appId;
        this.install = install;
    }

    public async Task<int> InstallBinary()
    {
        DirectoryInfo downloadDir = SystemDirectories.GetDownloadsPath(appId);
        string downloadPath = Path.Join(downloadDir.FullName, ".download");

        Console.WriteLine("Downloading...");
        await DownloadBinary(install.DownloadUrl, downloadPath);

        Console.WriteLine("Verifying...");

        byte[] expectedHash = Convert.FromBase64String(install.DownloadHash);
        byte[] downloadHash;
        await using (FileStream fs = File.OpenRead(downloadPath))
            downloadHash = await SHA256.HashDataAsync(fs);

        if (!expectedHash.AsSpan().SequenceEqual(downloadHash.AsSpan()))
        {
            Console.WriteLine("Downloaded file hash does not match the expected hash.");
            return 1;
        }

        Console.WriteLine("Installing...");
        throw new NotImplementedException();
    }

    private async Task DownloadBinary(Uri url, string filepath)
    {
        DownloadService downloadService = new();

        downloadService.DownloadStarted += DownloadService_DownloadStarted;
        downloadService.DownloadProgressChanged += DownloadService_DownloadProgressChanged;

        Console.WriteLine("Downloading...");
        await downloadService.DownloadFileTaskAsync(url.ToString(), filepath);
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
