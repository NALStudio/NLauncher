using NLauncher.Index.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications.Installs;
public class WebsiteAppInstall : AppInstall
{
    public required Uri Url { get; init; }
    public bool SupportsPwa { get; init; } // defaults to false

    public override Platforms GetSupportedPlatforms()
    {
        Platforms platforms = Platforms.Browser;
        if (SupportsPwa)
            platforms |= Platforms.Windows;

        return platforms;
    }
}
