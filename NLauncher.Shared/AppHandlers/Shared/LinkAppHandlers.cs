using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Shared;
public class StoreLinkAppHandler : LinkAppHandler<StoreLinkAppInstall>
{
    public override string GetHref(StoreLinkAppInstall install) => install.Url.ToString();
}

public class WebsiteLinkAppHandler : LinkAppHandler<WebsiteAppInstall>
{
    public override string GetHref(WebsiteAppInstall install) => install.Url.ToString();
}