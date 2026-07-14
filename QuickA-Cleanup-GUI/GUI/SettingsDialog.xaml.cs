using System.Diagnostics;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using QuickA_Cleanup.Core.Services;

namespace QuickA_Cleanup.GUI;

public sealed partial class SettingsDialog : ContentDialog
{
    private const string TestpinsUrl =
        "https://raw.githubusercontent.com/Ash1421/QuickA-Cleanup/main/testpins.reg";

    private readonly MainWindow _owner;
    private readonly LogService _log = LogService.Instance;
    private string? _downloadedRegPath;
    private bool _initializing = true;

    public SettingsDialog(MainWindow owner)
    {
        InitializeComponent();
        _owner = owner;

        ThemeButtons.SelectedIndex = ThemeManager.ThemeMode switch
        {
            AppThemeMode.Light => 0,
            AppThemeMode.Dark  => 1,
            _                  => 2
        };

        BuildAccentSwatches();

        LevelButtons.SelectedIndex = _log.MinLevel switch
        {
            LogLevel.Trace => 0,
            LogLevel.Dev   => 1,
            LogLevel.Warn  => 2,
            LogLevel.Error => 3,
            _              => 0
        };

        _initializing = false;

        ToggleRestartExplorer.IsOn = AppSettings.RestartExplorerAfterRemoval;

        var version = GetType().Assembly.GetName().Version;
        TxtCurrentVersion.Text = $"QuickA-Cleanup V{version?.ToString(3) ?? "3.0.1"}";
        UpdateService.StateChanged += RefreshUpdateUi;
        Closed += (_, _) => UpdateService.StateChanged -= RefreshUpdateUi;
        RefreshUpdateUi();

        TxtLogPath.Text = _log.LogFilePath;

        RefreshLogList();
        _log.Entries.CollectionChanged += (_, _) => RefreshLogList();
        RefreshTestpinStatus();
    }

    private void BuildAccentSwatches()
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        bool isWindowsAccent = ThemeManager.AccentSource == AccentMode.WindowsAccent;

        foreach (var swatch in ThemeManager.Swatches)
        {
            bool isActive = !isWindowsAccent && swatch.Color == ThemeManager.AccentColor;

            var circle = new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                Width = 26,
                Height = 26,
                Fill = new SolidColorBrush(swatch.Color),
                Stroke = isActive ? new SolidColorBrush(Colors.White) : null,
                StrokeThickness = 2
            };

            var button = new Button
            {
                Content = circle,
                Padding = new Thickness(2),
                CornerRadius = new CornerRadius(999),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Tag = swatch
            };
            ToolTipService.SetToolTip(button, swatch.Name);
            button.Click += AccentSwatch_Click;

            panel.Children.Add(button);
        }

        var matchWindowsButton = new Button
        {
            Content = "Match Windows",
            Padding = new Thickness(12, 6, 12, 6),
            Margin = new Thickness(8, 0, 0, 0),
            Style = isWindowsAccent
                ? (Style)Application.Current.Resources["AccentFilledButtonStyle"]
                : (Style)Application.Current.Resources["DefaultButtonStyle"]
        };
        matchWindowsButton.Click += (_, _) =>
        {
            ThemeManager.ApplyWindowsAccent();
            BuildAccentSwatches();
        };
        panel.Children.Add(matchWindowsButton);

        AccentSwatchList.Content = panel;
    }

    private void AccentSwatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AccentSwatch swatch }) return;
        ThemeManager.ApplyAccent(swatch.Color);
        BuildAccentSwatches();
    }

    private void ThemeButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;

        var mode = ThemeButtons.SelectedIndex switch
        {
            0 => AppThemeMode.Light,
            1 => AppThemeMode.Dark,
            _ => AppThemeMode.System
        };
        ThemeManager.ApplyTheme((FrameworkElement)_owner.Content, mode);
    }

    private void RefreshTestpinStatus()
    {
        int count = TestpinService.InstalledCount();

        if (count > 0)
        {
            TxtTestpinStatus.Text = $"{count} test entry/entries currently installed";
            BtnRemoveTestpins.IsEnabled = true;
        }
        else
        {
            TxtTestpinStatus.Text = "No test entries installed";
            BtnRemoveTestpins.IsEnabled = false;
        }
    }

    private void BtnRemoveTestpins_Click(object sender, RoutedEventArgs e)
    {
        _log.Dev("Removing testpins...");
        int removed = TestpinService.RemoveAll();
        SetTestStatus($"Removed {removed} test entry/entries", isError: false);
        _log.Dev($"Removed {removed} testpin entry/entries");
        RefreshTestpinStatus();
    }

    private async void BtnDownloadTestpins_Click(object sender, RoutedEventArgs e)
    {
        BtnDownloadTestpins.IsEnabled = false;
        SetTestStatus("Downloading testpins.reg...", isError: false);
        _log.Dev("Downloading testpins.reg from GitHub");

        try
        {
            using var http = new System.Net.Http.HttpClient();
            var content = await http.GetStringAsync(TestpinsUrl);
            _downloadedRegPath = Path.Combine(Path.GetTempPath(), "testpins.reg");
            await File.WriteAllTextAsync(_downloadedRegPath, content);

            SetTestStatus("Downloaded — click Install to import", isError: false);
            BtnInstallTestpins.IsEnabled = true;
            _log.Dev($"Testpins saved to {_downloadedRegPath}");
        }
        catch (Exception ex)
        {
            SetTestStatus($"Download failed: {ex.Message}", isError: true);
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
            SetTestStatus("No file downloaded yet — click Download first", isError: true);
            return;
        }

        try
        {
            _log.Dev($"Installing testpins from {_downloadedRegPath}");

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName        = "reg.exe",
                Arguments       = $"import \"{_downloadedRegPath}\"",
                UseShellExecute = false,
                CreateNoWindow  = true
            });
            process?.WaitForExit();

            SetTestStatus("Testpins installed — click Scan in the main window", isError: false);
            _log.Dev("Testpins installed successfully");
            RefreshTestpinStatus();
        }
        catch (Exception ex)
        {
            SetTestStatus($"Install failed: {ex.Message}", isError: true);
            _log.Error($"Testpins install failed: {ex.Message}");
        }
    }

    private void SetTestStatus(string message, bool isError)
    {
        TxtTestStatus.Text = message;
        TxtTestStatus.Foreground = isError ? StatusColors.Critical : StatusColors.Success;
    }

    private void LevelButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;

        _log.MinLevel = LevelButtons.SelectedIndex switch
        {
            0 => LogLevel.Trace,
            1 => LogLevel.Dev,
            2 => LogLevel.Warn,
            3 => LogLevel.Error,
            _ => LogLevel.Trace
        };
        _log.Dev($"Log level set to {_log.MinLevel}");
        RefreshLogList();
    }

    private void RefreshLogList()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var filtered = _log.Filtered.ToList();
            LogTextBox.Text = string.Join(Environment.NewLine, filtered.Select(entry => entry.Formatted));
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
            TxtLogCount.Text = $"{filtered.Count} entries";
        });
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        _log.Clear();
        RefreshLogList();
    }

    private void BtnCopyLogPath_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(_log.LogFilePath);
        Clipboard.SetContent(package);
    }

    private void RefreshUpdateUi()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            bool downloading = UpdateService.State == UpdateState.Downloading;
            UpdateSpinner.Visibility = downloading ? Visibility.Visible : Visibility.Collapsed;
            UpdateSpinner.IsActive = downloading;

            BtnDownloadUpdate.Visibility = UpdateService.State == UpdateState.Available ? Visibility.Visible : Visibility.Collapsed;
            BtnRestartToUpdate.Visibility = UpdateService.State == UpdateState.ReadyToInstall ? Visibility.Visible : Visibility.Collapsed;
            BtnCheckForUpdates.IsEnabled = !downloading;

            TxtUpdateStatus.Text = UpdateService.State switch
            {
                UpdateState.Unknown        => "Checking for updates...",
                UpdateState.UpToDate       => "You're on the latest version.",
                UpdateState.Available      => $"Update V{UpdateService.AvailableVersion} available.",
                UpdateState.Downloading    => $"Downloading V{UpdateService.AvailableVersion}...",
                UpdateState.ReadyToInstall => $"V{UpdateService.AvailableVersion} downloaded — restart to install.",
                UpdateState.NotInstalled   => "Updates aren't available for this copy (not installed via the installer).",
                UpdateState.Error          => "Update check failed — see logs.",
                _                          => ""
            };
        });
    }

    private async void BtnCheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        BtnCheckForUpdates.IsEnabled = false;
        await UpdateService.CheckAsync(logResult: true);
        BtnCheckForUpdates.IsEnabled = true;
    }

    private async void BtnDownloadUpdate_Click(object sender, RoutedEventArgs e) =>
        await UpdateService.DownloadAsync();

    private void BtnRestartToUpdate_Click(object sender, RoutedEventArgs e) =>
        UpdateService.ApplyAndRestart();

    private void ToggleRestartExplorer_Toggled(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;

        AppSettings.RestartExplorerAfterRemoval = ToggleRestartExplorer.IsOn;
        AppSettings.Save();
        _log.Dev($"Restart Explorer after removal set to {AppSettings.RestartExplorerAfterRemoval}");
    }
}
