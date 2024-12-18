
namespace NLauncher.Code.AppInstaller.Pipelines;
internal class BinaryInstallPipeline : AppInstallPipeline
{
    private readonly Uri downloadUrl;
    public BinaryInstallPipeline(Uri downloadUrl)
    {
        this.downloadUrl = downloadUrl;
    }

    public override Task InstallAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
