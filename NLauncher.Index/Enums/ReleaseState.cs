using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Enums;

// TODO: Use snake_case
[JsonConverter(typeof(JsonStringEnumConverter<AssetType>))]
public enum ReleaseState
{
    Released,
    EarlyAccess,
    NotReleased
}

public static class ReleaseStateEnum
{
    public static bool DownloadAllowed(this ReleaseState state) => state != ReleaseState.NotReleased;
}