using NLauncher.Windows.Models;
using Spectre.Console.Cli;
using System.IO.Pipes;

namespace NLauncher.Windows.Commands;
public abstract class BaseSettings : CommandSettings
{
    [CommandOption("-p|--pipe <PIPE_ID>")]
    public string? Pipe { get; set; }

    public async ValueTask<CommandOutput> ConnectOutputAsync()
    {
        NamedPipeClientStream? pipe;
        if (Pipe is not null)
        {
            Console.WriteLine($"Connecting to pipe: '{Pipe}'...");
            pipe = CreatePipe(Pipe);
            await pipe.ConnectAsync();
        }
        else
        {
            pipe = null;
        }

        return new CommandOutput(pipe);
    }

    public CommandOutput ConnectOutput()
    {
        NamedPipeClientStream? pipe;
        if (Pipe is not null)
        {
            Console.WriteLine($"Connecting to pipe: '{Pipe}'...");
            pipe = CreatePipe(Pipe);
            pipe.Connect();
        }
        else
        {
            pipe = null;
        }

        return new CommandOutput(pipe);
    }

    private static NamedPipeClientStream CreatePipe(string pipeId) => new(".", pipeId, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
}
