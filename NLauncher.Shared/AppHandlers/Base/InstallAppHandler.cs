using NLauncher.Index.Models.Applications.Installs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class InstallAppHandler : AppHandler
{
    /// <summary>
    /// Install the app asynchronously.
    /// </summary>
    /// <remarks>
    /// Note To Implementers: This function must be thread-safe.
    /// </remarks>
    public abstract IEnumerable<object> ConstructInstallPipeline(AppInstall install);
}

public abstract class InstallAppHandler<T> : InstallAppHandler where T : AppInstall
{
    public sealed override bool CanHandle(AppInstall install) => install is T;

    public sealed override IEnumerable<object> ConstructInstallPipeline(AppInstall install) => ConstructInstallPipeline((T)install);
    public abstract IEnumerable<object> ConstructInstallPipeline(T install);
}
