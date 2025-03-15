using NLauncher.Services.CheckUpdate;

namespace NLauncher.Web.Services;

public class WebCheckUpdate : ICheckUpdate
{
    public ValueTask<AvailableUpdate?> CheckForUpdateAsync() => ValueTask.FromResult<AvailableUpdate?>(null);
}
