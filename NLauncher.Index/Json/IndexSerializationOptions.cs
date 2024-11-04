using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Json;

[Flags]
public enum IndexSerializationOptions
{
    None = 0,

    WriteNulls = 1 << 0,
    WriteIndented = 1 << 1,

    HumanReadable = WriteNulls | WriteIndented,
}
