using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.FileChangeTree;
internal static class FileChangeTree
{
    /// <summary>
    /// Listen for changes.
    /// </summary>
    public static FileChangeTreeListener Listen(string root, FileChange? rootChange = null)
    {
        return new(root, rootChange: rootChange);
    }

    /// <summary>
    /// Listen for changes and write to <see cref="AnsiConsole"/> on dispose.
    /// </summary>
    public static FileChangeTreeListenAndRenderToConsoleWrapper ListenAndWrite(string root, FileChange? rootChange = null)
    {
        return new(root, rootChange: rootChange);
    }

    public static Tree Render(FileChangeTreeNode root)
    {
        return new FileChangeTreeRenderer(root).Render();
    }

    /// <summary>
    /// Construct a tree where <paramref name="change"/> is applied to all children of directory <paramref name="directoryPath"/>.
    /// </summary>
    public static FileChangeTreeNode ApplyToDirectory(string directoryPath, FileChange change)
    {
        DirectoryInfo dir = new(directoryPath);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Directory doesn't exist: '{directoryPath}'");

        FileChangeTreeNode root = new(dir.FullName, FileType.Directory, change);
        AddChildren(dir, root, change);

        return root;
    }

    private static void AddChildren(DirectoryInfo parentDirectory, FileChangeTreeNode parentNode, FileChange change)
    {
        foreach (FileSystemInfo child in parentDirectory.EnumerateFileSystemInfos())
        {
            FileType fileType;
            if (child is FileInfo)
                fileType = FileType.File;
            else if (child is DirectoryInfo)
                fileType = FileType.Directory;
            else
                fileType = FileType.Unknown;

            FileChangeTreeNode node = new(child.FullName, fileType, change);
            parentNode.Children.Add(node);

            if (child is DirectoryInfo di)
                AddChildren(di, node, change);
        }
    }
}
