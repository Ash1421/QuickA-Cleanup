using Velopack;
using Velopack.Exceptions;
using Velopack.Sources;

namespace QuickA_Cleanup.GUI;

public enum UpdateState { Unknown, UpToDate, Available, Downloading, ReadyToInstall, Error, NotInstalled }

public static class UpdateService
{
    private const string RepoUrl = "https://github.com/Ash1421/QuickA-Cleanup";
    private const string Channel = "win-x64";

    private static readonly LogService Log = LogService.Instance;
    private static UpdateManager? _manager;
    private static UpdateInfo? _pendingUpdate;

    public static UpdateState State { get; private set; } = UpdateState.Unknown;
    public static string? AvailableVersion { get; private set; }

    public static event Action? StateChanged;

    private static UpdateManager Manager =>
        _manager ??= new UpdateManager(
            new GithubSource(RepoUrl, accessToken: null, prerelease: false),
            new UpdateOptions { ExplicitChannel = Channel });

    public static void CheckSilentlyOnStartup()
    {
        _ = CheckAsync(logResult: true);
    }

    public static async Task<UpdateState> CheckAsync(bool logResult = false)
    {
        try
        {
            var info = await Manager.CheckForUpdatesAsync();
            if (info == null)
            {
                SetState(UpdateState.UpToDate, null);
                if (logResult) Log.Dev("Already on the latest version.");
                return UpdateState.UpToDate;
            }

            _pendingUpdate = info;
            SetState(UpdateState.Available, info.TargetFullRelease.Version.ToString());
            if (logResult) Log.Dev($"Update available: V{AvailableVersion}");
            return UpdateState.Available;
        }
        catch (NotInstalledException)
        {
            SetState(UpdateState.NotInstalled, null);
            return UpdateState.NotInstalled;
        }
        catch (Exception ex)
        {
            SetState(UpdateState.Error, null);
            Log.Error($"Update check failed: {ex.Message}");
            return UpdateState.Error;
        }
    }

    public static async Task<bool> DownloadAsync(Action<int>? progress = null)
    {
        if (_pendingUpdate == null) return false;

        try
        {
            SetState(UpdateState.Downloading, AvailableVersion);
            await Manager.DownloadUpdatesAsync(_pendingUpdate, progress);
            SetState(UpdateState.ReadyToInstall, AvailableVersion);
            Log.Dev($"Update V{AvailableVersion} downloaded, ready to install.");
            return true;
        }
        catch (Exception ex)
        {
            SetState(UpdateState.Error, AvailableVersion);
            Log.Error($"Update download failed: {ex.Message}");
            return false;
        }
    }

    public static void ApplyAndRestart()
    {
        if (_pendingUpdate == null) return;
        Log.Dev($"Applying update V{AvailableVersion} and restarting.");
        Manager.ApplyUpdatesAndRestart(_pendingUpdate);
    }

    private static void SetState(UpdateState state, string? version)
    {
        State = state;
        AvailableVersion = version;
        StateChanged?.Invoke();
    }
}
