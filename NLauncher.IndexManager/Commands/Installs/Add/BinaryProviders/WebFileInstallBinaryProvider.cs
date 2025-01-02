using Downloader;
using NLauncher.IndexManager.Components;
using Spectre.Console;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;

namespace NLauncher.IndexManager.Commands.Installs.Add.BinaryProviders;
internal class WebFileInstallBinaryProvider : InstallBinaryProvider
{
    public override string DisplayName => "Web File";

    public override async Task<FileData?> LoadFileDataAsync()
    {
        Uri url = AnsiConsole.Ask<Uri>("Download URL:");
        return await ComputeFileDataFromDownloads(url);
    }

    public static async Task<FileData?> ComputeFileDataFromDownloads(Uri downloadUrl)
    {
        const int hashCount = 2;

        Task<byte[]>[] hashTasks = new Task<byte[]>[hashCount];
        Task<string[]>? zipTask = null;

        await AnsiConsole.Progress()
            .Columns(
                new SpinnerColumn(),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new TransferSpeedColumn()
            )
            .StartAsync(async ctx =>
            {
                for (int i = 0; i < hashCount; i++)
                {
                    ProgressTask hashProg = ctx.AddTask($"Downloading file... (hash #{i + 1})");
                    hashTasks[i] = DownloadAndComputeHash(hashProg, downloadUrl);
                }

                ProgressTask zipProg = ctx.AddTask("Downloading file... (zip)");
                zipTask = DownloadAndGetFiles(zipProg, downloadUrl);

                await Task.WhenAll([.. hashTasks, zipTask]);
            });

        AnsiConsole.WriteLine("Verifying file hashes...");
        ReadOnlySpan<byte> expectedHash = hashTasks[0].Result.AsSpan();
        for (int i = 1; i < hashTasks.Length; i++)
        {
            if (!expectedHash.SequenceEqual(hashTasks[i].Result))
            {
                AnsiFormatter.WriteError("Downloaded files' hashes don't match.");
                return null;
            }
        }

        return new FileData(downloadUrl, expectedHash.ToImmutableArray(), zipTask!.Result.ToImmutableArray());
    }

    private static async Task<Stream> DownloadAsStream(ProgressTask progress, Uri downloadUrl)
    {
        void DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
        {
            progress.MaxValue(e.TotalBytesToReceive);
            progress.Value(e.ReceivedBytesSize);
        }

        IDownload download = DownloadBuilder.New()
            .WithUrl(downloadUrl)
            .Build();
        download.ChunkDownloadProgressChanged += DownloadProgressChanged;

        return await download.StartAsync();
    }

    private static async Task<byte[]> DownloadAndComputeHash(ProgressTask progress, Uri downloadUrl)
    {
        Stream downloadStream = await DownloadAsStream(progress, downloadUrl);
        return await SHA256.HashDataAsync(downloadStream);
    }

    private static async Task<string[]> DownloadAndGetFiles(ProgressTask progress, Uri downloadUrl)
    {
        Stream downloadStream = await DownloadAsStream(progress, downloadUrl);
        using ZipArchive zip = new(downloadStream, ZipArchiveMode.Read);
        return zip.Entries.Select(static e => e.FullName).ToArray();
    }
}
