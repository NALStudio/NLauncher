using NLauncher.Index.Models.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Models;
public readonly record struct DiscoveredManifest(string FilePath, string DirectoryPath, AppManifest Manifest);
