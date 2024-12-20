using NLauncher.Index.Models.InstallTracking;

namespace NLauncher.Services.Apps;
public interface IPlatformInstaller
{
    public abstract Task InstallAsync(InstallGuid installId, InstallProgress progress, CancellationToken cancellationToken);
}
