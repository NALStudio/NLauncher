using NLauncher.Index.Models.Index;
using System.Collections.Immutable;

namespace NLauncher.Code.IndexSearch;

public partial class IndexSearcher
{
    public string Search { get; }
    public IndexSearcher(string search)
    {
        Search = search;
    }

    private IEnumerable<IndexMatch> MatchAll(IndexManifest index)
    {
        foreach (IndexEntry entry in index.Entries)
        {
            yield return Match(entry);
        }
    }

    private bool MatchString(string s)
    {
        return s.StartsWith(Search, StringComparison.OrdinalIgnoreCase);
    }

    private bool ContainsString(string s)
    {
        return s.Contains(Search, StringComparison.OrdinalIgnoreCase);
    }

    private IndexMatch Match(IndexEntry entry)
    {
        bool nameMatch = MatchString(entry.Manifest.DisplayName);
        bool nameContains = ContainsString(entry.Manifest.DisplayName);
        bool developerMatch = MatchString(entry.Manifest.Developer);
        bool publisherMatch = MatchString(entry.Manifest.Publisher);

        return new IndexMatch(entry)
        {
            NameMatch = nameMatch,
            NameContains = nameContains,
            DeveloperMatch = developerMatch,
            PublisherMatch = publisherMatch
        };
    }

    public ImmutableArray<IndexEntry> SearchIndex(IndexManifest index)
    {
        return MatchAll(index).Where(static match => match.HasAnyMatches)
                              .Order() // sort using IComparable<IndexMatch> sort rules
                              .Select(static match => match.Entry)
                              .ToImmutableArray();
    }
}
