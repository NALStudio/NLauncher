using NLauncher.Index.Models.Applications.Installs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Base;
public abstract class LinkAppHandler : AppHandler
{
    public abstract string GetHref(AppInstall install);
}

public abstract class LinkAppHandler<T> : LinkAppHandler where T : AppInstall
{
    public sealed override bool CanHandle(AppInstall install) => install is T;

    public sealed override string GetHref(AppInstall install) => GetHref((T)install);
    public abstract string GetHref(T install);
}
