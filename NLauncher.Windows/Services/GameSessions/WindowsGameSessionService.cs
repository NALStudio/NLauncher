using NLauncher.Code.Json;
using NLauncher.Services.Sessions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NLauncher.Windows.Services.GameSessions;
public class WindowsGameSessionService : IGameSessionService
{
    private static readonly string SessionDirectory = Path.Join(WindowsStorageService.AppDataPath, "Sessions");
    private static readonly byte leftBracket = "["u8.ToArray().Single();

    private static string GetSessionFilePath(Guid appId)
    {
        if (!Directory.Exists(SessionDirectory))
            Directory.CreateDirectory(SessionDirectory);

        string filename = appId.ToString() + ".sessions";
        return Path.Join(SessionDirectory, filename);
    }

    public async ValueTask<GameSession[]?> LoadSessions(Guid appId)
    {
        string filepath = GetSessionFilePath(appId);

        FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await JsonSerializer.DeserializeAsync(stream, NLauncherJsonContext.Default.GameSessionArray);
    }

    internal static void WriteNewSessionSynchronous(FileStream stream, GameSession session)
    {
        Span<byte> singleByte = stackalloc byte[1];

        // Add [ to start if missing
        stream.Seek(0, SeekOrigin.Begin);
        int startingBracket = stream.ReadByte();
        if (startingBracket != leftBracket)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write("[\n"u8);
        }

        // Write value
        JsonSerializer.Serialize(stream, session, NLauncherJsonContext.Default.GameSession);
        stream.Write(",\n"u8);

        // TODO: Add missing ]
    }

    /// <summary>
    /// Returns <see langword="false"/> if another session is already running on the given handle.
    /// </summary>
    internal static bool TryStartSession(Guid appId, [MaybeNullWhen(false)] out GameSessionHandle handle)
    {
        string filepath = GetSessionFilePath(appId);

        // TODO: try-catch so that we can determine when session start failed (another session is already running)
        FileStream stream = File.Open(filepath, FileMode.Append, FileAccess.Write, FileShare.Read);

        handle = new(stream, disposeStream: true);
        return true;
    }
}
