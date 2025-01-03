namespace NLauncher.Services.Sessions;
public interface IGameSessionService
{
    /// <summary>
    /// Returns all of the recorded game sessions or <see langword="null"/> if not supported.
    /// </summary>
    public abstract ValueTask<GameSession[]?> LoadSessions(Guid appId);
}
