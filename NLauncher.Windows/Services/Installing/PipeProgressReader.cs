using NLauncher.Code.Models;
using NLauncher.Windows.Models;
using System.Diagnostics;
using System.IO.Pipes;

namespace NLauncher.Windows.Services.Installing;
internal class PipeProgressReader : IDisposable
{
    private readonly StreamReader reader;
    private readonly Action<InstallProgress> progressUpdate;

    private readonly CancellationTokenSource cancel;
    private Task? parseTask;

    public PipeProgressReader(NamedPipeServerStream server, Action<InstallProgress> progressUpdate)
    {
        reader = new(server);
        this.progressUpdate = progressUpdate;

        cancel = new CancellationTokenSource();
    }

    public void Start()
    {
        if (parseTask is not null)
            throw new InvalidOperationException("Parsing already started.");

        parseTask = new Task<Task>(ParseAsync, TaskCreationOptions.LongRunning);
        parseTask.Start();
    }

    public void Stop()
    {
        if (!cancel.IsCancellationRequested)
            cancel.Cancel();
    }

    private async Task ParseAsync()
    {
        while (true)
        {
            string? line = await reader.ReadLineAsync(cancel.Token);
            if (line is null)
                break;

            progressUpdate?.Invoke(ParseLine(line));
        }
    }

    private static InstallProgress ParseLine(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        Debug.Assert(!line.EndsWith('\n'));

        if (DownloadProgress.TryParse(line, out DownloadProgress p))
            return InstallProgress.Download(p.DownloadedBytes, p.TotalBytes);

        return InstallProgress.Indeterminate(line);
    }

    public void Dispose()
    {
        Stop();

        try
        {
            Debug.Assert(parseTask?.IsCompleted != false);
            parseTask?.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
        }

        reader.Dispose();
    }
}
