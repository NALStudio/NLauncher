using Spectre.Console;

namespace NLauncher.IndexManager.Components.FileChangeTree;
internal readonly ref struct FileChangeTreeRenderer(FileChangeTreeNode root)
{
    private readonly record struct ChangeData(string Name, FileType Type, FileChange? Change, FileChangeTreeNode ChangeNode)
    {
        public static int SortCompare(ChangeData a, ChangeData b)
        {
            // If types are equal, compare by name
            // otherwise, compare by type
            if (a.Type == b.Type)
                return a.Name.CompareTo(b.Name);
            else
                return a.Type.CompareTo(b.Type);
        }
    }

    public Tree Render()
    {
        ChangeData rootData = ExtractData(root);
        Tree tree = new(Render(rootData));
        AddChildrenToTree(tree, root);

        if (tree.Nodes.Count < 1)
            tree.AddNode(RenderNoChanges());

        return tree;
    }

    private static void AddChildrenToTree(Tree tree, FileChangeTreeNode treeNode)
    {
        List<(FileChangeTreeNode Node, IHasTreeNodes ConsoleNode)> nodes = [(treeNode, tree)];

        while (nodes.Count > 0)
        {
            (FileChangeTreeNode node, IHasTreeNodes consoleNode) = nodes[0];
            nodes.RemoveAt(0);

            ChangeData[] childData = node.Children.Select(static node => ExtractData(node)).ToArray();
            Array.Sort(childData, ChangeData.SortCompare); // Sort children so that they are ordered correctly in the file tree
            foreach (ChangeData data in childData)
            {
                TreeNode childConsoleNode = new(Render(data));
                consoleNode.AddNode(childConsoleNode);
                nodes.Add((data.ChangeNode, childConsoleNode));
            }
        }
    }

    private static ChangeData ExtractData(FileChangeTreeNode node)
    {
        string name = node.Name;
        FileChange? change = TrackChanges(node.Changes);

        FileType fileType = node.Type;
        if (fileType == FileType.Unknown)
        {
            // We don't know whether the given node is a file or directory
            // Try to guess the type based on whether the filename has an extension or not
            if (!string.IsNullOrEmpty(Path.GetExtension(node.FullPath)))
                fileType = FileType.File;
            else
                fileType = FileType.Directory;
        }

        return new ChangeData(name, fileType, change, node);
    }

    private static FileChange? TrackChanges(IEnumerable<FileChange> changes)
    {
        bool created = false;
        bool exists = true;
        bool modified = false;

        foreach (FileChange change in changes)
        {
            if (change == FileChange.Created)
            {
                created = true;
                exists = true;
            }
            else if (change == FileChange.Deleted)
            {
                exists = false;
            }
            else if (change == FileChange.Changed)
            {
                modified = true;
            }
            else
            {
                throw new ArgumentException($"Invalid change: '{change}' in changes.", nameof(changes));
            }
        }

        if (!exists)
            return FileChange.Deleted;
        if (created)
            return FileChange.Created;
        if (modified)
            return FileChange.Changed;
        return null;
    }

    private static Markup Render(ChangeData data)
    {
        string name = data.Name;

        string emoji = data.Type switch
        {
            FileType.File => Emoji.Known.PageFacingUp,
            FileType.Directory => Emoji.Known.FileFolder,
            _ => throw new ArgumentException($"Invalid filetype: '{data.Type}'")
        };

        string changeSymbol = data.Change switch
        {
            FileChange.Created => "+",
            FileChange.Changed => "□",
            FileChange.Deleted => "-",
            _ => string.Empty
        };

        string? changeColor = data.Change switch
        {
            FileChange.Created => "green",
            FileChange.Changed => "yellow",
            FileChange.Deleted => "red",
            _ => string.Empty
        };

        // Keep space after emoji even if emoji is null so that the changeSymbol doesn't cling onto the file name
        string markup = $"{changeSymbol}{emoji} {name}".EscapeMarkup();
        if (changeColor is not null)
            markup = $"[{changeColor}]{markup}[/]";

        return new Markup(markup);
    }

    private static Markup RenderNoChanges()
    {
        return new Markup("[grey italic]no changes done[/]");
    }
}
