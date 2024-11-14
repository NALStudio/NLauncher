using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Enums;

// TODO: Use snake_case
[JsonConverter(typeof(JsonStringEnumConverter<ReleaseState>))]
public enum ReleaseState
{
    [JsonStringEnumMemberName("released")]
    Released,

    [JsonStringEnumMemberName("early_access")]
    EarlyAccess,

    [JsonStringEnumMemberName("not_released")]
    NotReleased
}

public static class ReleaseStateEnum
{
    public static bool IsReleased(this ReleaseState state) => state != ReleaseState.NotReleased;
}