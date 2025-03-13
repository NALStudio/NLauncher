using NLauncher.Index.Models.Applications;

namespace NLauncher.Services.Apps.Installing;
public record Install(AppManifest App, AppVersion Version, InstallTask Task);
