﻿namespace NLauncher.Windows;
internal static class SystemDirectories
{
    private static DirectoryInfo GetSubDir(string dirname, Guid appId)
    {
        string? exeDir = Path.GetDirectoryName(Application.ExecutablePath);
        string dirpath = Path.Join(exeDir, dirname, appId.ToString());
        return new DirectoryInfo(dirpath);
    }

    public static DirectoryInfo GetLibraryPath(Guid appId) => GetSubDir("Library", appId);
    public static DirectoryInfo GetDownloadsPath(Guid appId) => GetSubDir("Downloads", appId);
}