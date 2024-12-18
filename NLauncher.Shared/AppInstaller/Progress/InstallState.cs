namespace NLauncher.Shared.AppInstaller.Progress;

public enum InstallState
{
    Starting,
    Downloading,
    Verifying,
    Extracting,

    Paused,
    Errored
}