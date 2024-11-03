using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components.FileChangeTree;
internal class FileChangeTreeNode
{
    public FileChangeTreeNode(string fullPath, FileType type)
    {
        Name = GetNameOfDirectoryOrFile(fullPath);
        FullPath = fullPath;

        Type = type;

        Changes = new();
        Children = new();
    }
    public FileChangeTreeNode(string fullPath, FileType type, FileChange change) : this(fullPath, type)
    {
        Changes.Add(change);
    }

    public string Name { get; }
    public string FullPath { get; }

    public FileType Type { get; }
    public List<FileChange> Changes { get; }

    public List<FileChangeTreeNode> Children { get; }

    /// <summary>
    /// <para>Get the name of the directory this path points to.</para>
    /// <para>GetNameOfDirectory("kaljaa/koneeseen/ja/ukkometsoon") == "ukkometsoon"</para>
    /// </summary>
    private static string GetNameOfDirectoryOrFile(string path)
    {
        // Yoinked from DirectoryInfo.Name implementation
        return Path.GetFileName(Path.TrimEndingDirectorySeparator(path));
    }
}
