using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Shared;
public class RecommendNLauncherAppHandler : AppHandler
{
    public override bool CanHandle(AppInstall install)
    {
        // Do not show "Install Using NLauncher" with links
        if (install is StoreLinkAppInstall)
            return false;
        if (install is WebsiteAppInstall)
            return false;

        return true;
    }
}
