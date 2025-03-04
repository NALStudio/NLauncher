using NLauncher.Services.Sessions;

namespace NLauncher.Web.Services;

public class WebGameSessionService : IGameSessionService
{
    public ValueTask<TimeSpan?> ComputeTotalTimeAsync(Guid appId) => ValueTask.FromResult<TimeSpan?>(null);

    public ValueTask<GameSession[]?> LoadSessionsAsync(Guid appId) => ValueTask.FromResult<GameSession[]?>(null);
}
