using NLauncher.Services.Sessions;

namespace NLauncher.Windows.Services.GameSessions;
internal class GameSessionHandle : IDisposable, IAsyncDisposable
{
    private readonly FileStream stream;
    private readonly bool disposeStream = false;

    private bool disposed = false;
    private bool sessionWritten = false;

    public GameSessionHandle(FileStream stream, bool disposeStream)
    {
        this.stream = stream;
        this.disposeStream = disposeStream;
    }

    public void WriteSessionSynchronous(GameSession session)
    {
        if (sessionWritten)
            throw new InvalidOperationException("A session was already written using this handle.");
        sessionWritten = true;

        if (disposed)
            throw new InvalidOperationException("Handle has been disposed.");

        WindowsGameSessionService.WriteNewSessionSynchronous(stream, session);
    }

    public void Dispose()
    {
        if (disposeStream)
            stream.Dispose();
        disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (disposeStream)
            await stream.DisposeAsync();
        disposed = true;
    }
}
