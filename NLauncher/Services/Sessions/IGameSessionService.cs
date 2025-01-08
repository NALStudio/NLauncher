namespace NLauncher.Services.Sessions;
public interface IGameSessionService
{
    /// <summary>
    /// Returns all of the recorded game sessions or <see langword="null"/> if not supported.
    /// </summary>
    /// <remarks>
    /// No guarantees are made of the order of the elements returned.
    /// </remarks>
    public abstract ValueTask<GameSession[]?> LoadSessionsAsync(Guid appId);

    /// <summary>
    /// Returns the total time the game has been played or <see langword="null"/> if the game hasn't been played even once.
    /// </summary>
    public abstract ValueTask<TimeSpan?> ComputeTotalTime(Guid appId);
}
