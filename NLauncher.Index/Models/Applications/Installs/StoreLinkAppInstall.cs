using NLauncher.Index.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications.Installs;
internal class StoreLinkAppInstall : AppInstall
{
    public required Platforms Platform { get; init; }
    public required Uri Url { get; init; }

    public override Platforms GetSupportedPlatforms() => Platform;
}
