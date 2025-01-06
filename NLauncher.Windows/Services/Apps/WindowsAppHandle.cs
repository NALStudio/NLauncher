using NLauncher.Services.Apps.Running;
using System.Diagnostics;

namespace NLauncher.Windows.Services.Apps;
public class WindowsAppHandle : AppHandle
{
    private readonly Process process;
    public WindowsAppHandle(Process process, bool startedSuccesfully)
    {
        if (!startedSuccesfully)
            throw new ArgumentException($"Only succesfully started processes should be passed to {nameof(WindowsAppHandle)}");

        this.process = process;
    }

    public override bool IsRunning => !process.HasExited;
    public override ValueTask KillAsync()
    {
        process.Kill(entireProcessTree: true);
        return ValueTask.CompletedTask;
    }
}
