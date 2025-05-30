﻿using NLauncher.Code.Language;

namespace NLauncher.Code.Models;
public readonly record struct InstallProgress
{
    public static InstallProgress Starting { get; } = Indeterminate("Starting...");

    public required string Status { get; init; }
    public required string? Message { get; init; }
    public required double? Progress { get; init; }

    public static InstallProgress Download(long? downloadedBytes, long? totalBytes)
    {
        static string? DownloadMessage(long downloadedBytes, long totalBytes)
        {
            string downloaded = HumanizeBinary.HumanizeBytes(downloadedBytes);
            string total = HumanizeBinary.HumanizeBytes(totalBytes);

            return $"{downloaded} / {total}";
        }

        downloadedBytes ??= 0L;

        string? message;
        double? progress;
        if (totalBytes.HasValue)
        {
            message = DownloadMessage(downloadedBytes.Value, totalBytes.Value);

            if (totalBytes.Value != 0L)
                progress = downloadedBytes.Value / (double)totalBytes.Value;
            else
                progress = 0;
        }
        else
        {
            message = null;
            progress = null;
        }

        return new InstallProgress()
        {
            Status = "Downloading...",
            Message = message,
            Progress = progress,
        };
    }

    public static InstallProgress Indeterminate(string status, string? message = null)
    {
        return new InstallProgress()
        {
            Status = status,
            Message = message,
            Progress = null
        };
    }
}
