using System.Text.Json.Serialization;

namespace NLauncher.Services.Sessions;
public class GameSession
{
    /// <summary>
    /// The UNIX timestamp (in seconds) for when the game session was started.
    /// </summary>
    public long Start { get; }

    /// <summary>
    /// The UNIX timestamp (in seconds) for when the game session was ended.
    /// </summary>
    public long End { get; }

    /// <summary>
    /// The amount of milliseconds that elapsed since the game was started.
    /// </summary>
    public long DurationMs { get; }

    [JsonConstructor]
    public GameSession(long start, long end, long durationMs)
    {
        Start = start;
        End = end;
        DurationMs = durationMs;
    }

    public GameSession(DateTimeOffset start, DateTimeOffset end) : this(start, end, duration: end - start)
    {
    }

    public GameSession(DateTimeOffset start, DateTimeOffset end, TimeSpan duration)
    {
        Start = start.ToUnixTimeSeconds();
        End = end.ToUnixTimeSeconds();
        DurationMs = duration.Ticks / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// <see cref="Start"/> represented as a <see cref="DateTime"/> in the local timezone.
    /// </summary>
    [JsonIgnore]
    public DateTime StartDateTime => DateTimeOffset.FromUnixTimeSeconds(Start).LocalDateTime;

    /// <summary>
    /// <see cref="End"/> represented as a <see cref="DateTime"/> in the local timezone.
    /// </summary>
    [JsonIgnore]
    public DateTime EndDateTime => DateTimeOffset.FromUnixTimeSeconds(End).LocalDateTime;

    [JsonIgnore]
    public TimeSpan Duration => TimeSpan.FromMilliseconds(DurationMs);
}
