using Velopack;
using Velopack.Sources;

namespace Nookly.Desktop.Services;

public sealed class AppUpdateService
{
    private readonly UpdateManager manager = new(
        new GithubSource("https://github.com/Kaiza42/nookly", null, false));

    public bool IsInstalled => manager.IsInstalled;
    public string CurrentVersion => manager.CurrentVersion?.ToString() ?? "developpement";
    public Task<UpdateInfo?> CheckAsync() => manager.CheckForUpdatesAsync();
    public Task DownloadAsync(UpdateInfo update, Action<int> progress, CancellationToken token = default) =>
        manager.DownloadUpdatesAsync(update, progress, token);
    public void ApplyAndRestart(UpdateInfo update) => manager.ApplyUpdatesAndRestart(update.TargetFullRelease);
}
