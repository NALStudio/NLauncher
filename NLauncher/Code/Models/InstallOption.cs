using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Base;

namespace NLauncher.Code.Models;

public readonly record struct InstallOption(AppHandler Handler, AppInstall Install)
{
    public static IEnumerable<InstallOption> EnumerateFromVersion<T>(AppVersion version, IEnumerable<T> appHandlers) where T : AppHandler
    {
        foreach (T handler in appHandlers)
        {
            foreach (AppInstall install in version.Installs)
            {
                if (handler.CanHandle(install))
                    yield return new InstallOption(handler, install);
            }
        }
    }
}