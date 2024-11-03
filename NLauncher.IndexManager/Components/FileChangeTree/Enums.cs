using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.FileChangeTree;
internal enum FileChange
{
    Created,
    Deleted,
    Changed
}

internal enum FileType
{
    Unknown,
    File,
    Directory
}
