using NLauncher.Code.AppInstaller.Pipelines;
using System.Runtime.Versioning;

namespace NLauncher.Shared.AppInstaller;

[SupportedOSPlatform("windows")]
public class AppInstaller
{
    // Limit the amount of concurrent installs so that we don't run out of sockets for downloads
    // We use a semaphore for the entire install so that when an install has started,
    // it does not need to wait in the middle of the install for a free download slot
    // if it needs to download multiple files
    private readonly SemaphoreSlim installSemaphore = new(0, 3);

    public void StartInstall(Guid appId, AppInstallPipeline pipeline)
    {
    }
}
