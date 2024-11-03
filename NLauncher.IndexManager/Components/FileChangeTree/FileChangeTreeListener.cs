using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NLauncher.IndexManager.Components.FileChangeTree;
internal class FileChangeTreeListener : IDisposable
{
    private readonly string rootFullPath;

    private readonly List<Exception> errors;

    private bool disposed = false;
    private FileChangeTreeNode? root;
    private readonly FileSystemWatcher watcher;

    public FileChangeTreeListener(string path, FileChange? rootChange = null)
    {
        errors = new();

        path = Path.GetFullPath(path);
        rootFullPath = path;

        if (rootChange.HasValue)
            root = new FileChangeTreeNode(path, FileType.Unknown, rootChange.Value);
        else
            root = new FileChangeTreeNode(path, FileType.Unknown);

        watcher = new(path)
        {
            IncludeSubdirectories = true
        };
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;

        watcher.EnableRaisingEvents = true;
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        PushChange(e.FullPath, FileChange.Created);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        // The FileWatcher docs have this check and I don't know why that is
        // This function is probably always called along with Create, Delete, Rename, etc. and thus we need to check this.
        if (e.ChangeType != WatcherChangeTypes.Changed)
            return;

        PushChange(e.FullPath, FileChange.Changed);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        PushChange(e.FullPath, FileChange.Deleted);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        PushChange(e.OldFullPath, FileChange.Deleted);
        PushChange(e.FullPath, FileChange.Created);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        errors.Add(e.GetException());
    }

    private void PushChange(string fullPath, FileChange change)
    {
        ThrowIfRootIsNull();

        FileChangeTreeNode parent = ConstructDirectoriesToPath(root, fullPath);

        // Handle last path element
        FileChangeTreeNode? node = parent.Children.FirstOrDefault(c => c.FullPath == fullPath);
        if (node is null)
        {
            node = new(fullPath, FileType.Unknown);

            Debug.Assert(!parent.Children.Any(c => c.Name == node.Name));
            parent.Children.Add(node);
        }

        node.Changes.Add(change);
    }

    /// <summary>
    /// Returns the parent directory of the file/directory at <paramref name="fullPath"/>
    /// </summary>
    private FileChangeTreeNode ConstructDirectoriesToPath(FileChangeTreeNode root, string fullPath)
    {
        string relative = PathAsRelative(fullPath);

        string[] path = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string reconstructedFullPath = rootFullPath;

        FileChangeTreeNode node = root;
        foreach (string part in path.SkipLast(1)) // Skip last, we want to handle this separately
        {
            reconstructedFullPath = Path.Join(reconstructedFullPath, part);

            FileChangeTreeNode? child = node.Children.FirstOrDefault(c => c.Name == part);
            if (child is null)
            {
                child = new(reconstructedFullPath, FileType.Unknown);
                node.Children.Add(child);
            }

            node = child;
        }

        return node;
    }

    /// <summary>
    /// Gets the provided <paramref name="fullPath"/> as a relative path relative to <root
    /// </summary>
    private string PathAsRelative(string fullPath)
    {
        string relative = Path.GetRelativePath(rootFullPath, fullPath);
        if (Path.IsPathRooted(relative))
            throw new ArgumentException($"Path '{fullPath}' is not a child of '{rootFullPath}'", nameof(fullPath));

        return relative;
    }

    [MemberNotNull(nameof(root))]
    private void ThrowIfRootIsNull()
    {
        if (root is null)
            throw new InvalidOperationException("Changes were already collected.");
    }

    /// <summary>
    /// Collect all of the changes that have happened before calling this function.
    /// </summary>
    /// <remarks>
    /// The call to this function disposes the <see cref="FileChangeTreeListener"/> and thus no new changes will be recorded.
    /// </remarks>
    public FileChangeTreeNode CollectChanges()
    {
        ThrowIfRootIsNull();

        // Dispose before collect so that no race conditions happen
        // where watcher registers a change even though no root exists.
        Dispose();

        FileChangeTreeNode root = this.root;
        this.root = null;

        return root;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        watcher.Dispose();
        disposed = true;
    }
}
