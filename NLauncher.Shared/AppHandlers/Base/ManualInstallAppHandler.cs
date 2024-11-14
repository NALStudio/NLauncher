using NLauncher.Index.Models.Applications.Installs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class ManualInstallAppHandler : AppHandler
{
    public abstract string GetDownloadLink(AppInstall install);
}

public abstract class ManualInstallAppHandler<T> : ManualInstallAppHandler where T : AppInstall
{
    public sealed override bool CanHandle(AppInstall install) => install is T;

    public sealed override string GetDownloadLink(AppInstall install) => GetDownloadLink((T)install);
    public abstract string GetDownloadLink(T install);
}
