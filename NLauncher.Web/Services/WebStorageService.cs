using Microsoft.JSInterop;
using NLauncher.Services;
using System.Runtime.Versioning;
using System.Text;

namespace NLauncher.Web.Services;

[SupportedOSPlatform("browser")]
public class WebStorageService : IStorageService
{
    private readonly IJSRuntime js;
    public WebStorageService(IJSRuntime js)
    {
        this.js = js;
    }

    private async ValueTask SetLocalStorageValue(string key, string value)
    {
        await js.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    private async ValueTask<string?> GetLocalStorageValue(string key)
    {
        return await js.InvokeAsync<string?>("localStorage.getItem", key);
    }

    private async ValueTask SetLocalStorageUtf8(string key, byte[] utf8)
    {
        // C# and JavaScript both use utf-16 strings
        // and LocalStorage doesn't accept any other type except strings.

        // So an unnecessary amount of boilerplate is needed
        // if I want to optimize this by sending the bytes raw
        // to JS side before doing the utf8 => utf16 convertsion.

        string value = Encoding.UTF8.GetString(utf8);
        await SetLocalStorageValue(key, value);
    }

    private async ValueTask<byte[]?> GetLocalStorageUtf8(string key)
    {
        string? value = await GetLocalStorageValue(key);
        if (value is null)
            return null;
        if (value.Length == 0) // UTF8.GetBytes doesn't do this optimization for some reason...
            return Array.Empty<byte>();
        return Encoding.UTF8.GetBytes(value);
    }

    private static string GetKey(string filename, bool cache = false)
    {
        IStorageService.ThrowIfFilenameInvalid(filename, requireExtension: !cache);
        string cachePath = cache ? ".cache" : string.Empty;
        return $"WebStorageService{cachePath}/{filename}";
    }

    public async ValueTask Write(string filename, byte[] utf8)
    {
        string key = GetKey(filename);
        await SetLocalStorageUtf8(key, utf8);
    }

    public async ValueTask Write(string filename, string value)
    {
        string key = GetKey(filename);
        await SetLocalStorageValue(key, value);
    }

    public async ValueTask WriteCache(string key, byte[] utf8)
    {
        string lsKey = GetKey(key, cache: true);
        await SetLocalStorageUtf8(lsKey, utf8);
    }

    public async ValueTask<byte[]> ReadUtf8(string filename)
    {
        string key = GetKey(filename);
        return await GetLocalStorageUtf8(key) ?? Array.Empty<byte>();
    }

    public async ValueTask<string> Read(string filename)
    {
        string key = GetKey(filename);
        return await GetLocalStorageValue(key) ?? string.Empty;
    }

    public async ValueTask<byte[]> ReadCache(string key)
    {
        string lsKey = GetKey(key, cache: true);
        return await GetLocalStorageUtf8(lsKey) ?? Array.Empty<byte>();
    }
}
