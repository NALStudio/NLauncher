using NLauncher.Index.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class IndexMeta : IIndexSerializable
{
    public required IndexRepository Repository { get; init; }
}
