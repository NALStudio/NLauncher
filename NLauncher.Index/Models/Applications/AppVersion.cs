using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications;
public class AppVersion
{
    public required uint VerNum { get; init; }
    public required string Identifier { get; init; }

    // Use immutable array instead of immutable list as this array will not get mutated ever.
    // Immutable list uses an AVL instead of an array to be more efficient during mutation, but thus its immutable performance suffers.
    public required ImmutableArray<AppInstall> Installs { get; init; }
}
