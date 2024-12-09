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
    protected override Uri GetUrl(StoreLinkAppInstall install) => install.Url;
}

public class WebsiteLinkAppHandler : LinkAppHandler<WebsiteAppInstall>
{
    protected override Uri GetUrl(WebsiteAppInstall install) => install.Url;
}