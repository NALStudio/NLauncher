using Downloader;
using NLauncher.Windows.Models;
using Spectre.Console.Cli;
using System.IO.Compression;
using System.Security.Cryptography;

namespace NLauncher.Windows.Commands.Install;

internal class BinaryInstallSettings : InstallSettings
{

    [CommandArgument(0, "<DOWNLOAD_URL>")]
    public required Uri DownloadUrl { get; init; }

    [CommandOption("--hash")]
    public string? DownloadHash { get; init; }
}

internal class BinaryInstallCommand : AsyncCommand<BinaryInstallSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BinaryInstallSettings settings)
    {
        await using CommandOutput output = await settings.ConnectOutputAsync();

        #region Downloading
        await output.WriteLineAsync("Downloading...");

        DirectoryInfo downloadDir = SystemDirectories.GetDownloadsPath(settings.AppId);
        string downloadPath = Path.Join(downloadDir.FullName, ".download");

        await DownloadBinary(output, settings.DownloadUrl, downloadPath);
        #endregion

        #region Verifying
        if (settings.DownloadHash is not null)
        {
            await output.WriteLineAsync("Verifying...");
            if (!await VerifyHash(downloadPath, settings.DownloadHash))
            {
                await output.WriteLineAsync("Downloaded file hash does not match the expected hash.");
                return 1;
            }
        }
        #endregion

        #region Installing
        await output.WriteLineAsync("Installing...");

        DirectoryInfo gameDir = SystemDirectories.GetLibraryPath(settings.AppId);
        DirectoryInfo zipTmpDir = new(Path.Join(downloadDir.FullName, "unzip"));
        Unzip(downloadPath, zipTmpDir, gameDir);
        #endregion

        #region Finishing
        // Message goes away too fast
        // await output.WriteLineAsync("Finishing...");
        downloadDir.Delete(true);
        #endregion

        return 0;
    }

    private static async Task DownloadBinary(CommandOutput output, Uri url, string filepath)
    {
        await using DownloadService downloadService = InstallHelper.CreateDefaultDownloadService();

        downloadService.DownloadStarted += async (sender, args) => await DownloadStarted(output, args);
        downloadService.DownloadProgressChanged += async (sender, args) => await DownloadProgressChanged(output, args);

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

    private static void Unzip(string zipPath, DirectoryInfo tmpDir, DirectoryInfo gameDir)
    {
        if (tmpDir.Exists)
            tmpDir.Delete(true);
        ZipFile.ExtractToDirectory(zipPath, tmpDir.FullName);

        FileSystemInfo[] infos = tmpDir.GetFileSystemInfos();
        DirectoryInfo moveDir;
        if (infos.Length == 1 && infos[0] is DirectoryInfo subTmpDir) // If extracted archive only contains a single directory, move directory upwards so that the games folder contains the actual gamefiles
            moveDir = subTmpDir;
        else
            moveDir = tmpDir;

        // Create directory or do nothing if it already exists
        // and then delete it with all its contents.
        // We do this to ensure that a) all parent directories exist, b) the game directory is empty and c) the game directory is deleted (so that we can move the extracted directory over)
        gameDir.Create();
        gameDir.Delete(true);

        moveDir.MoveTo(gameDir.FullName);
    }

    private static async Task DownloadStarted(CommandOutput output, DownloadStartedEventArgs e)
    {
        DownloadProgress progress = new(DownloadedBytes: 0, TotalBytes: e.TotalBytesToReceive);
        await output.WriteLineAsync(progress.ToString());
    }

    private static async Task DownloadProgressChanged(CommandOutput output, DownloadProgressChangedEventArgs e)
    {
        DownloadProgress progress = new(DownloadedBytes: e.ReceivedBytesSize, TotalBytes: e.TotalBytesToReceive);
        _ = await output.TryWriteLineAsync(progress.ToString());
    }
}
