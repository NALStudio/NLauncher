namespace NLauncher.Services.CheckUpdate;
public record class AvailableUpdate
{
    public required string CurrentVersion { get; init; }
    public required string AvailableVersion { get; init; }
}
