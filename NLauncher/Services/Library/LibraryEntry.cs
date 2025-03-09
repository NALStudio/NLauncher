using NLauncher.Index.Models.Applications.Installs;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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
    public LibraryInstallData? Install { get; init; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Install))]
    public bool IsInstalled => Install is not null;

    /// <summary>
    /// The chosen version's version number or <see langword="null"/> if no version is chosen (use latest version).
    /// </summary>
    [JsonPropertyName("vernum")]
    public uint? ChosenVerNum { get; init; }

    public string? LaunchOptions { get; init; }
}

public record class LibraryInstallData([property: JsonPropertyName("vernum")] uint VerNum, AppInstall Install);