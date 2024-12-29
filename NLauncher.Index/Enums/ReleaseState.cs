using System.Text.Json.Serialization;

namespace NLauncher.Index.Enums;

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