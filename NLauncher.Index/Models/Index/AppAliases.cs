﻿using NLauncher.Index.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLauncher.Index.Models.Index;
public class AppAliases
{
    public static readonly AppAliases Empty = new(ImmutableSortedDictionary<string, Guid>.Empty);

    // Only one safename can exist at a time, so dictionary is a perfect fit
    // Public so that the source generated JSON serializer can access this property
    public ImmutableSortedDictionary<string, Guid> Aliases { get; }

    // A Guid can have many safenames, use a lookup instead.
    private readonly ILookup<Guid, string> idToName;

    public AppAliases(ImmutableSortedDictionary<string, Guid> aliases)
    {
        Aliases = aliases;
        idToName = aliases.ToLookup(key => key.Value, value => value.Key);
    }

    /// <summary>
    /// Returns the UUID associated with the provided alias name or <see langword="null"/> if not found.
    /// </summary>
    public Guid? GetGuid(string name)
    {
        if (Aliases.TryGetValue(name, out Guid guid))
            return guid;
        return null;
    }

    public IEnumerable<string> GetNames(Guid id) => idToName[id];

    /// <summary>
    /// Returns the default name that is registered to this id or <see langword="null"/> if none are found.
    /// </summary>
    public string? GetName(Guid id) => GetNames(id).MaxBy(static name => name.Length);
}
