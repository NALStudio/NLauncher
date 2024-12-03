using Microsoft.JSInterop;
using NLauncher.Services;
using System.Runtime.Versioning;

namespace NLauncher.Web.Services;

[SupportedOSPlatform("browser")]
public partial class WebStorageService : IStorageService
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

    private static string GetLocalStorageKey(string filename)
    {
        IStorageService.ThrowIfFilenameInvalid(filename);
        return "NLauncher.Services.Storage.WebStorageService/" + filename;
    }

    public async ValueTask WriteAll(string filename, string text)
    {
        string key = GetLocalStorageKey(filename);
        await SetLocalStorageValue(key, text);
    }

    public async ValueTask<string> ReadAll(string filename)
    {
        string key = GetLocalStorageKey(filename);
        return await GetLocalStorageValue(key) ?? string.Empty;
    }
}
