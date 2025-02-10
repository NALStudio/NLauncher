namespace NLauncher.Services.CheckUpdate;
public interface ICheckUpdate
{
    public ValueTask<AvailableUpdate?> CheckForUpdateAsync();
}
