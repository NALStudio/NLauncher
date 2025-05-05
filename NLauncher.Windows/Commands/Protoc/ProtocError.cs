namespace NLauncher.Windows.Commands.Protoc;
public record class ProtocError(string Message)
{
    public static implicit operator ProtocError(string message) => new(message);
}
