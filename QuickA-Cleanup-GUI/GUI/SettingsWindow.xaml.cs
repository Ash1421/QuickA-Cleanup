using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace QuickA_Cleanup.GUI;

public partial class SettingsWindow : Window
{
    private const string TestpinsUrl =
        "https://raw.githubusercontent.com/Ash1421/QuickA-Cleanup/main/testpins.reg";

    private string? _downloadedRegPath;

    private readonly LogService _log = LogService.Instance;

    public SettingsWindow(Window owner)
    {
        InitializeComponent();
        Owner = owner;

        SizeToOwner();
        owner.LocationChanged += (_, _) => SizeToOwner();
        owner.SizeChanged     += (_, _) => SizeToOwner();

        RefreshLogList();
        _log.Entries.CollectionChanged += (_, _) => RefreshLogList();

        RefreshTestpinStatus();
    }

    // ── Sizing ───────────────────────────────────────────────────────────────

    private void SizeToOwner()
    {
        if (Owner == null) return;
        Left   = Owner.Left;
        Top    = Owner.Top;
        Width  = Owner.Width;
        Height = Owner.Height;
    }

    // ── Backdrop / close ─────────────────────────────────────────────────────

    private void Backdrop_MouseDown(object sender, MouseButtonEventArgs e) => Close();

    private void Card_MouseDown(object sender, MouseButtonEventArgs e) =>
        e.Handled = true; // prevent backdrop click-through

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void RefreshTestpinStatus()
    {
        int count = Core.Services.TestpinService.InstalledCount();
        if (count > 0)
        {
            TxtTestpinStatus.Text       = $"{count} test entry/entries currently installed";
            TxtTestpinStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xF5, 0x9E, 0x0B)); // amber
            BtnRemoveTestpins.IsEnabled = true;
            _log.Warn($"Testpins installed: {count} entry/entries found");
        }
        else
        {
            TxtTestpinStatus.Text       = "No test entries installed";
            TxtTestpinStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x8B, 0x8B, 0x9E)); // muted
            BtnRemoveTestpins.IsEnabled = false;
        }
    }

    private void BtnRemoveTestpins_Click(object sender, RoutedEventArgs e)
    {
        _log.Dev("Removing testpins...");
        int removed = Core.Services.TestpinService.RemoveAll();
        SetTestStatus($"✓ Removed {removed} test entry/entries", isError: false);
        _log.Dev($"Removed {removed} testpin entry/entries");
        RefreshTestpinStatus();
    }

    // ── Testing ──────────────────────────────────────────────────────────────

    private async void BtnDownloadTestpins_Click(object sender, RoutedEventArgs e)
    {
        BtnDownloadTestpins.IsEnabled = false;
        SetTestStatus("Downloading testpins.reg...", isError: false);
        _log.Dev("Downloading testpins.reg from GitHub");

        try
        {
            using var http   = new System.Net.Http.HttpClient();
            var content      = await http.GetStringAsync(TestpinsUrl);
            _downloadedRegPath = Path.Combine(Path.GetTempPath(), "testpins.reg");
            await File.WriteAllTextAsync(_downloadedRegPath, content);

            SetTestStatus($"✓ Downloaded — click Install to import", isError: false);
            BtnInstallTestpins.IsEnabled = true;
            _log.Dev($"Testpins saved to {_downloadedRegPath}");
        }
        catch (Exception ex)
        {
            SetTestStatus($"✗ Download failed: {ex.Message}", isError: true);
            _log.Error($"Testpins download failed: {ex.Message}");
        }
        finally
        {
            BtnDownloadTestpins.IsEnabled = true;
        }
    }

    private void BtnInstallTestpins_Click(object sender, RoutedEventArgs e)
    {
        if (_downloadedRegPath == null || !File.Exists(_downloadedRegPath))
        {
            SetTestStatus("✗ No file downloaded yet — click Download first", isError: true);
            return;
        }

        try
        {
            _log.Dev($"Installing testpins from {_downloadedRegPath}");

            Process.Start(new ProcessStartInfo
            {
                FileName        = "reg.exe",
                Arguments       = $"import \"{_downloadedRegPath}\"",
                UseShellExecute = false,
                CreateNoWindow  = true,
                Verb            = "runas"
            })?.WaitForExit();

            SetTestStatus("✓ Testpins installed — click Scan in the main window", isError: false);
            _log.Dev("Testpins installed successfully");
            RefreshTestpinStatus();
        }
        catch (Exception ex)
        {
            SetTestStatus($"✗ Install failed: {ex.Message}", isError: true);
            _log.Error($"Testpins install failed: {ex.Message}");
        }
    }

    private void SetTestStatus(string message, bool isError)
    {
        TxtTestStatus.Text       = message;
        TxtTestStatus.Foreground = isError
            ? new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44))
            : new System.Windows.Media.SolidColorBrush(
                  System.Windows.Media.Color.FromRgb(0x10, 0xB9, 0x81));
    }

    // ── Log level buttons ─────────────────────────────────────────────────────

    private void LevelButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton clicked) return;

        // Uncheck all others — only one active at a time
        foreach (var btn in new[] { BtnLevelTrace, BtnLevelDev, BtnLevelWarn, BtnLevelError })
            btn.IsChecked = btn == clicked;

        _log.MinLevel = clicked.Tag?.ToString() switch
        {
            "Trace" => LogLevel.Trace,
            "Dev"   => LogLevel.Dev,
            "Warn"  => LogLevel.Warn,
            "Error" => LogLevel.Error,
            _       => LogLevel.Trace
        };

        _log.Dev($"Log level set to {_log.MinLevel}");
        RefreshLogList();
    }

    // ── Log viewer ────────────────────────────────────────────────────────────

    private void RefreshLogList()
    {
        Dispatcher.Invoke(() =>
        {
            var filtered      = _log.Filtered.ToList();
            LogList.ItemsSource  = null;
            LogList.ItemsSource  = filtered;
            TxtLogCount.Text     = $"{filtered.Count} entries";

            // Auto-scroll to bottom
            LogScrollViewer.ScrollToBottom();
        });
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        _log.Clear();
        RefreshLogList();
    }
}
