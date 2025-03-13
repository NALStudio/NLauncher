using System.IO.Pipes;

namespace NLauncher.Windows.Models;
public class CommandOutput : IDisposable, IAsyncDisposable
{
    private static readonly TimeSpan throttleDelay = TimeSpan.FromMilliseconds(500);

    private readonly NamedPipeClientStream? pipe;
    private readonly StreamWriter? writer;

    private readonly CancellationTokenSource? writeReleaseCancel;
    private Task? releaseWrite;

    private readonly SemaphoreSlim? writeLock;

    public CommandOutput(NamedPipeClientStream? pipe)
    {
        this.pipe = pipe;
        if (pipe is not null)
        {
            writeReleaseCancel = new();

            writer = new StreamWriter(pipe);
            writeLock = new(1, 1);
        }
    }

    /// <summary>
    /// This write will be throttled if necessary to keep the pipe from clogging up.
    /// </summary>
    public async ValueTask WriteLineAsync(string line)
    {
        if (writeLock is not null)
            await writeLock.WaitAsync();
        await UnthrottledWriteLineAsync(line);
        DelayedReleaseWriteLock();
    }

    /// <summary>
    /// This write will fail instead of throttle.
    /// </summary>
    public async ValueTask<bool> TryWriteLineAsync(string line)
    {
        if (writeLock?.Wait(millisecondsTimeout: 0) == false)
            return false;

        await UnthrottledWriteLineAsync(line);
        DelayedReleaseWriteLock();

        return true;
    }

    public void WriteLine(string line)
    {
        writeLock?.Wait();
        UnthrottledWriteLine(line);
        DelayedReleaseWriteLock();
    }

    public bool TryWriteLine(string line)
    {
        if (writeLock?.Wait(millisecondsTimeout: 0) == false)
            return false;

        UnthrottledWriteLine(line);
        DelayedReleaseWriteLock();

        return true;
    }

    private void UnthrottledWriteLine(string line)
    {
        if (writer is not null)
        {
            writer.WriteLine(line);
            writer.Flush();
        }

        Console.WriteLine(line);
    }

    private async ValueTask UnthrottledWriteLineAsync(string line)
    {

        if (writer is not null)
        {
            await writer.WriteLineAsync(line);
            await writer.FlushAsync();
        }

        Console.WriteLine(line);
    }

    private void DelayedReleaseWriteLock()
    {
        static async Task ReleaseSem(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await Task.Delay(throttleDelay, cancellationToken);
            semaphore.Release();
        }

        if (writeLock is not null)
            releaseWrite = ReleaseSem(writeLock, writeReleaseCancel!.Token);
    }


    public void Dispose()
    {
        writeReleaseCancel?.Cancel();
        writeLock?.Dispose();
        pipe?.Dispose();
        writer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        writeReleaseCancel?.Cancel();
        writeLock?.Dispose();

        // Dispose writer before pipe so it can flush everything out
        if (writer is not null)
            await writer.DisposeAsync();
        if (pipe is not null)
            await pipe.DisposeAsync();
    }
}
