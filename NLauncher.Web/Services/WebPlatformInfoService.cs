using NLauncher.Services;

namespace NLauncher.Web.Services;

public class WebPlatformInfoService : IPlatformInfoService
{
    public int? PrimaryScreenWidth => null;
    public int? PrimaryScreenHeight => null;

    /*
    public ValueTask<int> CurrentScreenWidth => js.InvokeAsync<int>("window.screen.width");
    public ValueTask<int> CurrentScreenHeight => js.InvokeAsync<int>("window.screen.height");
    public ValueTask<int> AvailableWidth => js.InvokeAsync<int>("window.screen.availWidth");
    public ValueTask<int> AvailableHeight => js.InvokeAsync<int>("window.screen.availHeight");
    */
}
