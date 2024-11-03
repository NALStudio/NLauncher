using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.FileChangeTree;
internal class FileChangeTreeListenAndRenderToConsoleWrapper : IDisposable
{
    private bool disposed = false;
    private readonly FileChangeTreeListener listener;

    public FileChangeTreeListenAndRenderToConsoleWrapper(string path, FileChange? rootChange)
    {
        listener = new(path, rootChange: rootChange);
    }

    public void Dispose()
    {
        if (disposed)
            return;

        FileChangeTreeNode node = listener.CollectChanges();
        listener.Dispose();

        Tree rendered = FileChangeTree.Render(node);
        AnsiConsole.Write(rendered);

        disposed = true;
    }
}
