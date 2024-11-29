using NLauncher.Index.Models.Applications;
using NLauncher.Index.Models.Index;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Shared.Helpers;
public static class AppManifestSorter
{
    // We don't inherit from Comparer<AppManifest> since it throws an error that AppManifest must define IComparable
    public class Comparer : IComparer<AppManifest>
    {
        public static readonly Comparer Instance = new();

        public int Compare(AppManifest? a, AppManifest? b)
        {
            // Compare null (sort null as last)
            if (a is null && b is null)
                return 0;
            if (a is null) // b is not null
                return 1;
            if (b is null) // a is not null
                return -1;
            // a is not null && b is not null

            // Compare values
            return CompareFunc(a, b);
        }

        public static int CompareFunc(AppManifest a, AppManifest b)
        {
            // Try to compare priorities first
            // Compare b to a to sort from largest priority to smallest
            int comparePriority = b.Priority.CompareTo(a.Priority);
            if (comparePriority != 0)
                return comparePriority;

            // If priorities are equal, sort alphabetically
            return a.DisplayName.CompareTo(b.DisplayName);
        }
    }

    public static ImmutableArray<IndexEntry> Sort(IEnumerable<IndexEntry> entries) => Sort(entries, static entry => entry.Manifest);
    public static ImmutableArray<T> Sort<T>(IEnumerable<T> manifests, Func<T, AppManifest> manifestSelector)
    {
        int Compare(T a, T b)
        {
            AppManifest am = manifestSelector(a);
            AppManifest bm = manifestSelector(b);
            return Comparer.CompareFunc(am, bm);
        }

        T[] entriesArr = manifests.ToArray();
        Array.Sort(entriesArr, Compare);
        return ImmutableCollectionsMarshal.AsImmutableArray(entriesArr);
    }
}
