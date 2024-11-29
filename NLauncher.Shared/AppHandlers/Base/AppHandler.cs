using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Shared;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class AppHandler
{
    public abstract bool CanHandle(AppInstall install);

    public static readonly ImmutableArray<AppHandler> SharedHandlers = [
        new StoreLinkAppHandler(),
        new WebsiteLinkAppHandler(),
        new BinaryManualInstallAppHandler()
    ];
}
