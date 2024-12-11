using Microsoft.Extensions.Logging;
using NLauncher.Code;
using NLauncher.Code.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NLauncher.Services.Library;
public class LibraryService
{
    private readonly ILogger<LibraryService> logger;
    private readonly IStorageService storage;
    public LibraryService(ILogger<LibraryService> logger, IStorageService storage)
    {
        this.logger = logger;
        this.storage = storage;
    }

    private readonly SemaphoreSlim entriesSemaphore = new(1, 1);
    private Dictionary<Guid, LibraryEntry>? entries;

    public async Task<IEnumerable<LibraryEntry>> EnumerateEntriesAsync()
    {
        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();
            return entries!.Values;
        }
        finally
        {
            entriesSemaphore.Release();
        }
    }

    public async Task<bool> HasEntryForApp(Guid appId)
    {
        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();
            return entries!.ContainsKey(appId);
        }
        finally
        {
            entriesSemaphore.Release();
        }
    }

    /// <summary>
    /// Update entry's timestamp to the current time.
    /// </summary>
    public async Task UpdateEntryAsync(Guid appId) => await UpdateEntryAsync(appId, static data => data);

    /// <summary>
    /// Update an entry's data. If no existing data is found, a default <see cref="LibraryData"/> instance will be provided to <paramref name="updateFunc"/>.
    /// </summary>
    /// <remarks>
    /// Entry's timestamp will be updated to the current time.
    /// </remarks>
    public async Task UpdateEntryAsync(Guid appId, Func<LibraryData, LibraryData> updateFunc)
    {
        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();

            LibraryData oldData;
            if (entries!.Remove(appId, out LibraryEntry oldEntry))
                oldData = oldEntry.Data;
            else
                oldData = new(); // If no data found, create new data with default values.

            LibraryData newData = updateFunc(oldData);
            entries.Add(appId, new LibraryEntry(GetTimestamp(), newData));

            await SaveEntries();
        }
        finally
        {
            entriesSemaphore.Release();
        }
    }

    public async Task AddEntryAsync(Guid appId, LibraryData? initialData = null)
    {
        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();

            initialData ??= new();
            entries!.Add(appId, new LibraryEntry(GetTimestamp(), initialData));

            await SaveEntries();
        }
        finally
        {
            entriesSemaphore.Release();
        }
    }

    public async Task<bool> RemoveEntryAsync(Guid appId)
    {
        bool removed;

        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();
            removed = entries!.Remove(appId);
            await SaveEntries();
        }
        finally
        {
            entriesSemaphore.Release();
        }

        return removed;
    }

    private static long GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private async Task TryLoadEntries()
    {
        // Only load entries if they haven't been loaded already
        if (this.entries is not null)
            return;

        Dictionary<Guid, LibraryEntry>? entries = null;
        try
        {
            string json = await storage.ReadAll(NLauncherConstants.FileNames.Library);
            if (!string.IsNullOrEmpty(json))
                entries = JsonSerializer.Deserialize(json, NLauncherJsonContext.Default.DictionaryGuidLibraryEntry) ?? throw new Exception("entries was null.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Library entries load failed.");
        }

        this.entries = entries ?? new Dictionary<Guid, LibraryEntry>();
        Debug.Assert(this.entries is not null);
    }

    private async Task SaveEntries()
    {
        if (entries is null)
            throw new InvalidOperationException("No entries loaded.");

        string json = JsonSerializer.Serialize(entries, NLauncherJsonContext.Default.DictionaryGuidLibraryEntry);
        await storage.WriteAll(NLauncherConstants.FileNames.Library, json);
    }
}
