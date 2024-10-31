using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications.Installs;
public class BinaryAppInstall : AppInstall
{
    public required Uri DownloadUrl { get; init; }
    public required string DownloadHash { get; init; }
    public required string ExecutablePath { get; init; }
}
