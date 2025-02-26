﻿using Microsoft.Extensions.Logging;
using NLauncher.Code.Json;
using NLauncher.Services.Sessions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace NLauncher.Windows.Services.GameSessions;
public class WindowsGameSessionService : IGameSessionService
{
    private static readonly string SessionDirectory = Path.Join(WindowsStorageService.AppDataPath, "Sessions");
    private static readonly byte newline = "\n"u8.ToArray().Single();

    private readonly ILogger<WindowsGameSessionService> logger;
    public WindowsGameSessionService(ILogger<WindowsGameSessionService> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<GameSession[]?> LoadSessionsAsync(Guid appId)
    {
        try
        {
            List<GameSession> sessions = new();
            await foreach (GameSession gs in InternalLoadSessions(appId))
                sessions.Add(gs);
            return sessions.ToArray();
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Game session file could not be opened.");
            return null;
        }
    }

    public async ValueTask<TimeSpan?> ComputeTotalTime(Guid appId)
    {
        long totalMs = 0L;

        try
        {
            await foreach (GameSession gs in InternalLoadSessions(appId))
                totalMs += gs.DurationMs;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Game session file could not be opened.");
            return null;
        }

        return TimeSpan.FromMilliseconds(totalMs);
    }

    private async IAsyncEnumerable<GameSession> InternalLoadSessions(Guid appId)
    {
        string filepath = GetSessionFilePath(appId);
        await using FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader sr = new(stream, Encoding.UTF8);

        string? line;
        // Using cancellation token changes type from Task to ValueTask
        while (!string.IsNullOrEmpty(line = await sr.ReadLineAsync(CancellationToken.None)))
        {
            bool sessionDeserializeError = false;
            GameSession? session = null;
            try
            {
                session = JsonSerializer.Deserialize(line, NLauncherJsonContext.Default.GameSession);
            }
            catch (Exception ex)
            {
                sessionDeserializeError = true;
                logger.LogError(ex, "GameSession could not be deserialized.");
            }

            if (session is not null)
                yield return session;
            else if (!sessionDeserializeError)
                logger.LogError("GameSession deserialized as null.");
        }
    }

    private static string GetSessionFilePath(Guid appId)
    {
        if (!Directory.Exists(SessionDirectory))
            Directory.CreateDirectory(SessionDirectory);

        string filename = appId.ToString() + ".sessions";
        return Path.Join(SessionDirectory, filename);
    }

    internal static void WriteNewSessionSynchronous(FileStream stream, GameSession session)
    {
        ReadOnlySpan<byte> sessionBytes = JsonSerializer.SerializeToUtf8Bytes(session, NLauncherJsonContext.Default.GameSession);
        if (sessionBytes.Contains(newline))
            throw new ArgumentException("Session serialization resulted in newlines.");

        // Nothing was written if I don't seek
        stream.Seek(0, SeekOrigin.End);

        stream.Write(sessionBytes);
        stream.WriteByte(newline);
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
