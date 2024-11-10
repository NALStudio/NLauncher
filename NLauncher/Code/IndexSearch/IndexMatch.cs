using NLauncher.Index.Models.Index;

namespace NLauncher.Code.IndexSearch;

public partial class IndexSearcher
{
    private readonly struct IndexMatch : IComparable<IndexMatch>
    {
        public IndexEntry Entry { get; }

        public IndexMatch(IndexEntry entry)
        {
            Entry = entry;
        }

        public required bool NameMatch { get; init; }
        public required bool NameContains { get; init; }

        public required bool DeveloperMatch { get; init; }
        public required bool PublisherMatch { get; init; }

        public bool HasAnyMatches => NameMatch || NameContains || DeveloperMatch || PublisherMatch;

        /// <summary>
        /// Compare bools inverse of what <see cref="bool.CompareTo(bool)"/> would do so that true values precede false values in the list.
        /// </summary>
        private static int CompareBools(bool a, bool b) => b.CompareTo(a);

        public int CompareTo(IndexMatch other)
        {
            // Try compare name match
            int nameCompare = CompareBools(NameMatch, other.NameMatch);
            if (nameCompare != 0)
                return nameCompare;

            // Try compare name contains
            int containsCompare = CompareBools(NameContains, other.NameContains);
            if (containsCompare != 0)
                return containsCompare;

            // Try compare developer match
            int developerCompare = CompareBools(DeveloperMatch, other.DeveloperMatch);
            if (developerCompare != 0)
                return developerCompare;

            // Try compare publisher
            int publisherCompare = CompareBools(PublisherMatch, other.PublisherMatch);
            if (publisherCompare != 0)
                return publisherCompare;

            // Fallback to alphabetical sorting if matches are equal
            return Entry.Manifest.DisplayName.CompareTo(other.Entry.Manifest.DisplayName);
        }
    }
}
