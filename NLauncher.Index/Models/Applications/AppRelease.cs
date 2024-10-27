using NLauncher.Index.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications;
public class AppRelease
{
    public required ReleaseState State { get; init; }
    public required DateOnly? ReleaseDate { get; init; }
}
