using NLauncher.Index.Models.Applications.Installs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLauncher.Services.Library;

// Last updated timestamp should be compared with the last played timestamp of an installed game
// as we don't have access to LibraryService in our 
public readonly record struct LibraryEntry(Guid AppId, long LastUpdatedTimestamp, LibraryData Data);

// Use record to allow for with {} statement usage
// if we ever plan on adding any data to this object
public record class LibraryData
{
    public AppInstall? Install { get; }
}