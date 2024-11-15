using NLauncher.IndexManager.Components.Paths;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.IndexManager.Components;
internal static class NewsSlots
{
    private const int SlotCount = 6;

    private static string GetDirectoryPath(IndexPaths index, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        return Path.Join(index.NewsDirectory, slotIndex.ToString(CultureInfo.InvariantCulture));
    }

    public static NewsPaths GetPath(IndexPaths index, int slotIndex)
    {
        return new NewsPaths(GetDirectoryPath(index, slotIndex));
    }

    public static IEnumerable<(int Index, NewsPaths Paths)> EnumeratePathIndexes(IndexPaths index, bool existingOnly = false)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            NewsPaths paths = GetPath(index, i);
            if (existingOnly && !paths.Exists())
                continue;
            yield return (i, paths);
        }
    }
    public static IEnumerable<NewsPaths> EnumeratePaths(IndexPaths index, bool existingOnly = false)
    {
        foreach ((int _, NewsPaths paths) in EnumeratePathIndexes(index, existingOnly: existingOnly))
            yield return paths;
    }
}
