using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace NLauncher.Services.Storage;

[SupportedOSPlatform("browser")]
public partial class WebStorageService : IStorageService
{
    [JSImport("setLocalStorageValue", "WebStorageService")]
    private static partial void SetLocalStorageValue(string value);

    [JSImport("getLocalStorageValue", "WebStorageService")]
    private static partial string GetLocalStorageValue();

    private bool imported = false;
    private async ValueTask TryImportModule()
    {
        if (!imported)
        {
            await JSHost.ImportAsync("WebStorageService", "/js/WebStorageServiceJS.js");
            imported = true;
        }
    }

    public async ValueTask WriteAll(string text)
    {
        await TryImportModule();
        SetLocalStorageValue(text);
    }

    public async ValueTask<string> ReadAll()
    {
        await TryImportModule();
        return GetLocalStorageValue();
    }
}
