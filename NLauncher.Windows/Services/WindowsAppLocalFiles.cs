using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps;
using System.Diagnostics;

namespace NLauncher.Windows.Services;
public class WindowsAppLocalFiles : IAppLocalFiles
{
    public long? ComputeSizeInBytes(Guid appId, AppInstall existingInstall)
    {
        DirectoryInfo dir = SystemDirectories.GetLibraryPath(appId);
        if (!dir.Exists)
            return null;

        long byteSize = 0L;
        foreach (FileInfo file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            byteSize += file.Length;

        return byteSize;
    }

    public bool OpenFileBrowserSupported(AppInstall existingInstall) => existingInstall is BinaryAppInstall;
    public ValueTask<bool> OpenFileBrowserAsync(Guid appId, AppInstall existingInstall)
    {
        if (existingInstall is not BinaryAppInstall)
            return ValueTask.FromResult(false);

        DirectoryInfo dir = SystemDirectories.GetLibraryPath(appId);
        if (!dir.Exists)
            return ValueTask.FromResult(false);

        // Hopefully the path is not malicious, I have no way to check really other than that the directory actually exists
        // I try to be a bit more secure by using explorer instead of running the path manually... Explorer will still open a file if one exists though...
        Process.Start("explorer.exe", [dir.FullName]);
        return ValueTask.FromResult(true);
    }
}
