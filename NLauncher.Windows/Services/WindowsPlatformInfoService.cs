using NLauncher.Services;

namespace NLauncher.Windows.Services;
public class WindowsPlatformInfoService : IPlatformInfoService
{
    public int? PrimaryScreenWidth => SystemInformation.PrimaryMonitorSize.Width;
    public int? PrimaryScreenHeight => SystemInformation.PrimaryMonitorSize.Height;
}
