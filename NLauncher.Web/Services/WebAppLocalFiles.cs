using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Services.Apps;

namespace NLauncher.Web.Services;

public class WebAppLocalFiles : IAppLocalFiles
{
    public long? ComputeSizeInBytes(Guid appId, AppInstall existingInstall) => null;
    public ValueTask<bool> OpenFileBrowserAsync(Guid appId, AppInstall existingInstall) => ValueTask.FromResult(false);
    public bool OpenFileBrowserSupported(AppInstall existingInstall) => false;
}
