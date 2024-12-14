﻿using NLauncher.Index.Models.Applications.Installs;

namespace NLauncher.Services.Library;

// Last updated timestamp should be compared with the last played timestamp of an installed game
// as we don't have access to LibraryService in our 
public readonly record struct LibraryEntry(Guid AppId, long LastUpdatedTimestamp, LibraryData Data);

// Use record to allow for with {} statement usage
// if we ever plan on adding any data to this object
public record class LibraryData
{
    /// <summary>
    /// The install of the application or <see langword="null"/> if the application has not yet been installed.
    /// </summary>
    /// <remarks>
    /// If the app is not installed, it can still be in the library as a website link for example.
    /// </remarks>
    public AppInstall? Install { get; init; }

    /// <summary>
    /// The chosen version's version number or <see langword="null"/> if no version is chosen (use latest version).
    /// </summary>
    public uint? VerNum { get; init; }
}