using NLauncher.Index.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Applications;
public class AppRelease
{
    public required ReleaseState State { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)] // Always write the release date in the JSON output regardless of value
    public required DateOnly? ReleaseDate { get; init; }
}
