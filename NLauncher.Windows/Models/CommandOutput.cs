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
        try
        {
            await UnthrottledWriteLineAsync(line);
        }
        finally
        {
            DelayedReleaseWriteLock();
        }
    }

    /// <summary>
    /// This write will fail instead of throttle.
    /// </summary>
    public async ValueTask<bool> TryWriteLineAsync(string line)
    {
        if (writeLock?.Wait(millisecondsTimeout: 0) == false)
            return false;
        try
        {
            await UnthrottledWriteLineAsync(line);
        }
        finally
        {
            DelayedReleaseWriteLock();
        }

        return true;
    }

    public void WriteLine(string line)
    {
        writeLock?.Wait();
        try
        {
            UnthrottledWriteLine(line);
        }
        finally
        {
            DelayedReleaseWriteLock();
        }
    }

    public bool TryWriteLine(string line)
    {
        if (writeLock?.Wait(millisecondsTimeout: 0) == false)
            return false;
        try
        {
            UnthrottledWriteLine(line);
        }
        finally
        {
            DelayedReleaseWriteLock();
        }

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
        static async Task ThrottleReleaseSem(SemaphoreSlim semaphore, TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }

        if (writeLock is not null)
            releaseWrite = ThrottleReleaseSem(writeLock, throttleDelay, writeReleaseCancel!.Token);
    }


    public void Dispose()
    {
        // Cancel
        writeReleaseCancel?.Cancel();

        try
        {
            // Wait until the write lock has been released (before we dispose it)
            releaseWrite?.Wait();
        }
        catch (OperationCanceledException)
        {
        }

        // Dispose
        writeLock?.Dispose();
        writer?.Dispose();
        pipe?.Dispose();

        // Finalize
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        // Cancel
        writeReleaseCancel?.Cancel();

        try
        {
            if (releaseWrite is not null)
                await releaseWrite.WaitAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
        }

        // Dispose
        writeLock?.Dispose();

        // Dispose Async
        if (writer is not null) // Dispose writer before pipe so it can flush everything out
            await writer.DisposeAsync();
        if (pipe is not null)
            await pipe.DisposeAsync();

        // Finalize
        GC.SuppressFinalize(this);
    }
}
