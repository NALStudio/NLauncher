namespace NLauncher.Windows.Commands.Protoc;
internal abstract class ProtocCommand
{
    public abstract Task<ProtocError?> ExecuteAsync(string[] args);
}
