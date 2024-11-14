using NLauncher.Index.Models.Applications.Installs;
using NLauncher.Shared.AppHandlers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.AppHandlers.Shared;
public class BinaryManualInstallAppHandler : ManualInstallAppHandler<BinaryAppInstall>
{
    public override string GetDownloadLink(BinaryAppInstall install) => install.DownloadUrl.ToString();
}
