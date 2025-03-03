using Microsoft.Extensions.Logging;
using NLauncher.Code.Json;
using NLauncher.Services.Sessions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NLauncher.Windows.Services.GameSessions;
public class WindowsGameSessionService : IGameSessionService
{
    private static readonly string SessionDirectory = Path.Join(WindowsStorageService.AppDataPath, "Sessions");

    private readonly ILogger<WindowsGameSessionService> logger;
    public WindowsGameSessionService(ILogger<WindowsGameSessionService> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<GameSession[]?> LoadSessionsAsync(Guid appId)
    {
        List<GameSession>? sessions = await LoadAndAggregateSessions<List<GameSession>>(appId, state: new(), static (sessions, sess) =>
        {
            sessions.Add(sess);
            return sessions;
        });
        return sessions?.ToArray();
    }

    public async ValueTask<TimeSpan?> ComputeTotalTimeAsync(Guid appId)
    {
        long? totalMs = await LoadAndAggregateSessions<long?>(appId, state: 0L, static (ms, session) =>
        {
            long msValue = ms!.Value; // Initial state is not null.
            return msValue + session.DurationMs;
        });

        if (totalMs.HasValue)
            return TimeSpan.FromMilliseconds(totalMs.Value);
        else
            return null;
    }

    /// <summary>
    /// Returns `default(T)` if no data could be loaded.
    /// </summary>
    private async ValueTask<T?> LoadAndAggregateSessions<T>(Guid appId, T state, Func<T, GameSession, T> aggregate)
    {
        string filepath = GetSessionFilePath(appId);
        if (!File.Exists(filepath))
            return default;

        try
        {
            await using FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await foreach (GameSession? session in JsonSerializer.DeserializeAsyncEnumerable(stream, NLauncherJsonContext.Default.GameSession, topLevelValues: true))
            {
                if (session is null)
                    logger.LogError("GameSession deserialized as null.");
                else
                    state = aggregate(state, session);
            }
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Game session file could not be opened.");
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Game sessions failed to deserialize.");
            return default;
        }

        return state;
    }

    private static string GetSessionFilePath(Guid appId)
    {
        if (!Directory.Exists(SessionDirectory))
            Directory.CreateDirectory(SessionDirectory);

        string filename = appId.ToString() + ".sessions";
        return Path.Join(SessionDirectory, filename);
    }

    internal static void WriteNewSession(FileStream stream, GameSession session)
    {
        stream.Seek(0, SeekOrigin.End);
        JsonSerializer.Serialize(stream, session, NLauncherJsonContext.Default.GameSession);
        stream.WriteByte((byte)'\n');
    }

    internal static async Task WriteNewSessionAsync(FileStream stream, GameSession session)
    {
        stream.Seek(0, SeekOrigin.End);
        await JsonSerializer.SerializeAsync(stream, session, NLauncherJsonContext.Default.GameSession);
        stream.WriteByte((byte)'\n');
    }

    /// <summary>
    /// Returns <see langword="false"/> if another session is already running on the given handle.
    /// </summary>
    internal static bool TryStartSession(Guid appId, [MaybeNullWhen(false)] out GameSessionHandle handle)
    {
        string filepath = GetSessionFilePath(appId);

        FileStream stream;
        try
        {
            stream = File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }
        catch (IOException)
        {
            handle = null;
            return false;
        }

        handle = new(stream, disposeStream: true);
        return true;
    }
}
