﻿using Downloader;

namespace NLauncher.Windows.Commands.Default.Install;
public static class InstallHelper
{
    public static DownloadService CreateDefaultDownloadService()
    {
        return new DownloadService(
            new DownloadConfiguration()
            {
                BufferBlockSize = 8000,
                ReserveStorageSpaceBeforeStartingDownload = true,
            }
        );
    }
}
