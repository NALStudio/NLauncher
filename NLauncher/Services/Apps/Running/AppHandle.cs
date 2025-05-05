namespace NLauncher.Services.Apps.Running;
public abstract class AppHandle
{
    public abstract bool IsRunning { get; }
    public abstract ValueTask KillAsync();
}
