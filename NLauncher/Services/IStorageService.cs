namespace NLauncher.Services;

/// <summary>
/// This is the common storage service shared between web and Windows.
/// <para>WEB: Uses local storage</para>
/// <para>WINDOWS: Uses AppData</para>
/// </summary>
/// <remarks>
/// <para>
/// A directory for downloading, unzipping, etc. should be provided by a different service.
/// This 'Downloads' directory should be with the game folder somewhere else and not in AppData.
/// </para>
/// </remarks>
public interface IStorageService
{
    /// <summary>
    /// Write all text to file overriding any previous content that might exist.
    /// </summary>
    public ValueTask Write(string filename, byte[] utf8);

    /// <inheritdoc cref="Write"/>
    public ValueTask Write(string filename, string value);

    /// <summary>
    /// Write all text to cache file at the given key overriding any previous content that might exist.
    /// </summary>
    /// <remarks>
    /// Data is written into a separate directory on Windows.
    /// </remarks>
    public ValueTask WriteCache(string key, byte[] utf8);

    /// <summary>
    /// Read all text from file.
    /// </summary>
    /// <remarks>
    /// Returns an empty byte array if file does not exist.
    /// </remarks>
    public ValueTask<byte[]> ReadUtf8(string filename);

    /// <summary>
    /// <inheritdoc cref="Read"/>
    /// </summary>
    /// <remarks>
    /// Returns an empty string if file does not exist.
    /// </remarks>
    public ValueTask<string> Read(string filename);

    /// <summary>
    /// Read all text from cache file at the given key.
    /// </summary>
    /// <remarks>
    /// Returns an empty byte array if file does not exist.
    /// </remarks>
    public ValueTask<byte[]> ReadCache(string key);

    /// <summary>
    /// Throw if <paramref name="filename"/> is not a valid file name.
    /// </summary>
    /// <remarks>
    /// Protects against passing in paths that might resolve to another location on disk.
    /// </remarks>
    protected static void ThrowIfFilenameInvalid(string filename, bool requireExtension = true)
    {
        // If any invalid characters are found (slashes, etc.), throw
        int invalidCharIndex = filename.IndexOfAny(Path.GetInvalidFileNameChars());
        if (invalidCharIndex != -1)
            throw new ArgumentException("Invalid filename");

        // If filename doesn't have an extension, throw
        // filename is guaranteed to be a single filename at this point
        if (requireExtension && Path.GetExtension(filename.AsSpan()).IsEmpty)
            throw new ArgumentException("Filename doesn't have an extension.");
    }
}
