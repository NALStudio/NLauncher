namespace NLauncher.Services;

/// <summary>
/// This is the common storage service shared between web and Windows.
/// <para>WEB: Uses local storage</para>
/// <para>WINDOWS: Uses AppData</para>
/// </summary>
/// <remarks>
/// A directory for downloading, unzipping, etc. should be provided by a different service.
/// This 'Downloads' directory should be with the game folder somewhere else and not in AppData.
/// </remarks>
public interface IStorageService
{
    /// <summary>
    /// Write all text to file overriding any previous content that might exist.
    /// </summary>
    public ValueTask WriteAll(string filename, string text);

    /// <summary>
    /// Read all text from file.
    /// </summary>
    /// <remarks>
    /// Returns an empty string if file does not exist.
    /// </remarks>
    public ValueTask<string> ReadAll(string filename);

    /// <summary>
    /// Throw if <paramref name="filename"/> is not a valid file name.
    /// </summary>
    /// <remarks>
    /// Protects against passing in paths that might resolve to another location on disk.
    /// </remarks>
    protected static void ThrowIfFilenameInvalid(string filename)
    {
        int invalidCharIndex = filename.IndexOfAny(Path.GetInvalidFileNameChars());
        if (invalidCharIndex != -1)
            throw new ArgumentException("Invalid filename");
    }
}
