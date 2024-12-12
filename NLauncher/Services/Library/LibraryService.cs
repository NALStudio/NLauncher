using Microsoft.Extensions.Logging;
using NLauncher.Code;
using NLauncher.Code.Json;
using System.Collections.Immutable;
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

    /// <summary>
    /// Fired when any entry change happens.
    /// </summary>
    public event Action? EntriesChanged;

    public async Task<LibraryEntry[]> GetEntriesAsync()
    {
        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();

            return entries!.Values.ToArray(); // Must make it into an array so that the enumerable doesn't escape the semaphore's scope
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
        // Get timestamp before awaiting so that entries are ordered in the call order and not in the order the lock is acquired.
        long timestamp = GetTimestamp();

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
            LibraryEntry newEntry = new(appId, timestamp, newData);
            entries.Add(appId, newEntry);

            await SaveEntries();
        }
        finally
        {
            entriesSemaphore.Release();
        }

        EntriesChanged?.Invoke();
    }

    /// <summary>
    /// Adds the specified app UUID to the library and returns the added entry.
    /// If application already exists, the existing entry will be returned instead
    /// </summary>
    public async Task<LibraryEntry> AddEntryAsync(Guid appId)
    {
        // Get timestamp before awaiting so that entries are ordered in the call order and not in the order the lock is acquired.
        LibraryEntry entry = new(appId, GetTimestamp(), new LibraryData());

        await entriesSemaphore.WaitAsync();
        try
        {
            await TryLoadEntries();

            // If entry wasn't added, return existing entry instead
            if (!entries!.TryAdd(appId, entry))
                entry = entries[appId];

            await SaveEntries();
        }
        finally
        {
            entriesSemaphore.Release();
        }

        EntriesChanged?.Invoke();
        return entry;
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

        if (removed)
            EntriesChanged?.Invoke();

        return removed;
    }

    private static long GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private async Task TryLoadEntries()
    {
        // Only load entries if they haven't been loaded already
        if (entries is not null)
            return;

        entries = await Task.Run(async () => await InternalLoadEntries(storage, logger));
    }

    private static async Task<Dictionary<Guid, LibraryEntry>> InternalLoadEntries(IStorageService storage, ILogger<LibraryService> logger)
    {
        LibraryEntry[]? entries = null;
        try
        {
            string json = await storage.ReadAll(NLauncherConstants.FileNames.Library);
            if (!string.IsNullOrEmpty(json))
                entries = JsonSerializer.Deserialize(json, NLauncherJsonContext.Default.LibraryEntryArray) ?? throw new Exception("entries was null.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Library entries load failed.");
        }

        entries ??= Array.Empty<LibraryEntry>();
        return entries.ToDictionary(key => key.AppId);
    }

    private async Task SaveEntries()
    {
        if (entries is null)
            throw new InvalidOperationException("No entries loaded.");

        string json = JsonSerializer.Serialize(entries.Values, NLauncherJsonContext.Default.IEnumerableLibraryEntry);
        await storage.WriteAll(NLauncherConstants.FileNames.Library, json);
    }
}
