namespace NLauncher.Index.Models.InstallTracking;
public class InstallProgress
{
    private readonly Lock @lock = new();

    private bool errored = false;
    private string? state;
    private double? progress = null;

    public event Action? StateChanged;

    public (bool Errored, string? State, double? Progress) GetState()
    {
        lock (@lock)
        {
            return (errored, state, progress);
        }
    }

    public void UpdateState(string state, double? progress = null)
    {
        lock (@lock)
        {
            ThrowIfErrored();
            this.state = state;
            this.progress = progress;
        }

        StateChanged?.Invoke();
    }

    public void UpdateProgress(double progress)
    {
        lock (@lock)
        {
            ThrowIfErrored();
            this.progress = progress;
        }

        StateChanged?.Invoke();
    }

    public void Error()
    {
        lock (@lock)
        {
            ThrowIfErrored();
            errored = true;
        }

        StateChanged?.Invoke();
    }

    private void ThrowIfErrored()
    {
        if (errored)
            throw new InvalidOperationException("Install is in an errored state.");
    }
}
