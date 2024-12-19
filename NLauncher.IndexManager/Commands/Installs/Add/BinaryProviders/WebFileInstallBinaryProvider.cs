using Downloader;
using NLauncher.IndexManager.Components;
using Spectre.Console;
using System.Collections.Immutable;
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

    public static async Task<FileData?> ComputeFileDataFromDownloads(Uri downloadUrl, int downloadCount = 2)
    {
        Task<byte[]>[] tasks = new Task<byte[]>[downloadCount];

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
                for (int i = 0; i < downloadCount; i++)
                {
                    ProgressTask prog = ctx.AddTask($"Downloading file #{i}...");
                    tasks[i] = DownloadAndComputeHash(prog, downloadUrl);
                }

                await Task.WhenAll(tasks);
            });

        AnsiConsole.WriteLine("Verifying file hashes...");
        ReadOnlySpan<byte> expectedHash = tasks[0].Result.AsSpan();
        for (int i = 1; i < tasks.Length; i++)
        {
            if (!expectedHash.SequenceEqual(tasks[i].Result))
            {
                AnsiFormatter.WriteError("Downloaded files' hashes don't match.");
                return null;
            }
        }

        return new FileData(downloadUrl, expectedHash.ToImmutableArray());
    }

    private static async Task<byte[]> DownloadAndComputeHash(ProgressTask progress, Uri downloadUrl)
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

        Stream downloadStream = await download.StartAsync();
        return await SHA256.HashDataAsync(downloadStream);
    }
}
