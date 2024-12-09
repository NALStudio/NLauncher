using NLauncher.Index.Models.Applications.Installs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class LinkAppHandler : AppHandler
{
    public abstract string GetHref(AppInstall install);
    public abstract Uri GetUrl(AppInstall install);
}

public abstract class LinkAppHandler<T> : LinkAppHandler where T : AppInstall
{
    public sealed override bool CanHandle(AppInstall install) => install is T;

    public sealed override string GetHref(AppInstall install) => GetUrl(install).ToString();
    public sealed override Uri GetUrl(AppInstall install) => GetUrl((T)install);

    protected abstract Uri GetUrl(T install);
}
