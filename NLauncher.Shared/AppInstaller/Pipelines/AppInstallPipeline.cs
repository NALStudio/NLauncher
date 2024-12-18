namespace NLauncher.Code.AppInstaller.Pipelines;
public abstract class AppInstallPipeline
{
    public abstract Task InstallAsync(CancellationToken cancellationToken);
}
