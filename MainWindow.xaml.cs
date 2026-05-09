using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using QuickA_Cleanup.Core.Models;
using QuickA_Cleanup.Core.Services;

namespace QuickA_Cleanup.GUI;

public partial class MainWindow : Window
{
    private List<ItemViewModel> _viewModels = new();
    private readonly LogService _log = LogService.Instance;
    private Storyboard? _testpinPulse;

    public MainWindow()
    {
        InitializeComponent();
        var version = GetType().Assembly.GetName().Version;
        VersionBadge.Text = $"v{version?.ToString(3) ?? "2.0.0"}";

        _log.Trace("MainWindow initialised");

        // Watch for new errors to update dot
        _log.Entries.CollectionChanged += (_, _) => UpdateStatusDots();

        CheckTestpinsOnStartup();
    }

    // ── Status dots ──────────────────────────────────────────────────────────

    private void UpdateStatusDots(bool? hasBloat = null)
    {
        Dispatcher.Invoke(() =>
        {
            // Error dot
            bool hasErrors = _log.Entries.Any(e => e.Level == LogLevel.Error);
            DotError.Fill    = hasErrors
                ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                : new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
            DotError.ToolTip = hasErrors ? "Errors in log — open Settings" : "Errors: none";
            LblError.Foreground = hasErrors
                ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                : new SolidColorBrush(Color.FromRgb(0x8B, 0x8B, 0x9E));

            // Bloat dot
            if (hasBloat.HasValue)
            {
                DotBloat.Fill    = hasBloat.Value
                    ? new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B))
                    : new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                DotBloat.ToolTip = hasBloat.Value ? "Bloatware detected" : "Bloat: clean";
                LblBloat.Foreground = hasBloat.Value
                    ? new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B))
                    : new SolidColorBrush(Color.FromRgb(0x8B, 0x8B, 0x9E));
            }

            // Testpin dot — hidden when not installed, yellow+pulsing when installed
            bool testpinsInstalled = TestpinService.AreInstalled();
            DotTestpinGroup.Visibility = testpinsInstalled ? Visibility.Visible : Visibility.Collapsed;

            if (testpinsInstalled)
            {
                // Start ease-in-out pulse if not already running
                if (_testpinPulse == null)
                {
                    _testpinPulse = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
                    var anim = new DoubleAnimation(1, 0.2, new Duration(TimeSpan.FromSeconds(1.2)))
                    {
                        AutoReverse    = true,
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };
                    Storyboard.SetTarget(anim, DotTestpin);
                    Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
                    _testpinPulse.Children.Add(anim);
                    _testpinPulse.Begin();
                }
            }
            else
            {
                _testpinPulse?.Stop();
                _testpinPulse = null;
                DotTestpin.Opacity = 1;
            }
        });
    }

    private void CheckTestpinsOnStartup()
    {
        int count = TestpinService.InstalledCount();
        if (count > 0)
        {
            _log.Warn($"Testpins detected — {count} test entry/entries installed.");
            TxtStatus.Text = $"⚠ {count} test entry/entries detected — open Settings to remove.";
        }
        else
        {
            _log.Trace("No testpins detected on startup");
        }
        UpdateStatusDots();
    }

    // ── Title bar ────────────────────────────────────────────────────────────

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        _log.Trace("Settings opened");
        var settings = new SettingsWindow(this);
        settings.Closed += (_, _) => UpdateStatusDots(); // refresh dot when settings closes
        settings.ShowDialog();
    }

    // ── Scan ─────────────────────────────────────────────────────────────────

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        ShowOverlay("Scanning registry...", "Looking for Quick Access entries");
        ShowScanProgress(true);
        BtnScan.IsEnabled = false;
        _log.Dev("Registry scan started");

        try
        {
            var scanner = new RegistryScanner();
            var (items, skipped) = await Task.Run(() => scanner.ScanForItems());

            _log.Dev($"Scan complete — {items.Count} found, {skipped} protected skipped");
            foreach (var item in items)
                _log.Trace($"  Found: {item.Name} [{item.Guid}]" +
                           (item.Tags.Contains("known") ? " [bloatware]" : ""));

            // Tag testpins
            foreach (var item in items)
                if (ItemFilter.IsKnownItem(item.Guid) &&
                    item.Name.StartsWith("TEST_PIN", StringComparison.OrdinalIgnoreCase))
                    item.Tags.Add("testpin");

            _viewModels = items.Select(i => new ItemViewModel(i)).ToList();
            foreach (var vm in _viewModels) vm.SelectionChanged += UpdateSelectionCount;

            ItemsList.ItemsSource = _viewModels;
            EmptyState.Visibility = _viewModels.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            bool hasBloat = items.Any(i => i.Tags.Contains("known") && !i.Tags.Contains("testpin"));
            TxtStatus.Text = _viewModels.Count == 0
                ? "No removable items found."
                : $"{_viewModels.Count} item(s) found  •  {skipped} protected hidden" +
                  (hasBloat ? "  •  ⚠ bloatware detected" : "");

            if (hasBloat)
                UpdateStatusDots(hasBloat: true);
            else
                UpdateStatusDots();

            UpdateSelectionCount();
        }
        catch (Exception ex)
        {
            _log.Error($"Scan failed: {ex.Message}");
            TxtStatus.Text = "Scan failed — check logs in Settings.";
            UpdateStatusDots();
        }
        finally
        {
            HideOverlay();
            ShowScanProgress(false);
            BtnScan.IsEnabled = true;
        }
    }

    // ── Selection ────────────────────────────────────────────────────────────

    private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var vm in _viewModels) vm.IsSelected = true;
        _log.Trace("All items selected");
    }

    private void BtnDeselectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var vm in _viewModels) vm.IsSelected = false;
        _log.Trace("All items deselected");
    }

    private void UpdateSelectionCount()
    {
        int count = _viewModels.Count(v => v.IsSelected);
        TxtSelectionCount.Text = $"{count} selected";
        BtnRemove.IsEnabled    = count > 0;
    }

    // ── Row hover ────────────────────────────────────────────────────────────

    private void Row_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border b && b.DataContext is ItemViewModel vm && !vm.IsSelected)
            b.Background = new SolidColorBrush(Color.FromRgb(0x23, 0x23, 0x2B));
    }

    private void Row_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border b && b.DataContext is ItemViewModel vm && !vm.IsSelected)
            b.Background = Brushes.Transparent;
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    private async void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        var toRemove = _viewModels.Where(v => v.IsSelected).ToList();
        bool dryRun  = ChkDryRun.IsChecked == true;
        bool backup  = ChkBackup.IsChecked == true;

        _log.Dev($"Remove requested — {toRemove.Count} item(s), dryRun={dryRun}, backup={backup}");

        var result = MessageBox.Show(
            $"Are you sure you want to {(dryRun ? "simulate removal of" : "remove")} {toRemove.Count} item(s)?",
            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            _log.Trace("Removal cancelled by user");
            return;
        }

        ShowOverlay(dryRun ? "Simulating..." : "Removing items...", $"0 / {toRemove.Count} complete");
        BtnRemove.IsEnabled = false;

        try
        {
            if (backup && !dryRun)
                await RunBackupAsync(toRemove);

            var (ok, fail) = await RunRemovalLoopAsync(toRemove, dryRun);

            if (!dryRun)
                await FinishRemovalAsync();

            string msg = dryRun
                ? $"[Dry Run] Would have removed {ok} item(s)."
                : $"Done — {ok} removed, {fail} failed.";

            _log.Dev(msg);
            TxtStatus.Text = msg;
            UpdateStatusDots();
            MessageBox.Show(msg, "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _log.Error($"Removal error: {ex.Message}");
            UpdateStatusDots();
        }
        finally
        {
            HideOverlay();
            UpdateSelectionCount();
        }
    }

    private async Task RunBackupAsync(List<ItemViewModel> toRemove)
    {
        var path = await Task.Run(() =>
            BackupService.CreateBackup(toRemove.Select(v => v.Item), AppContext.BaseDirectory));
        if (path != null)
        {
            TxtStatus.Text = $"Backup saved → {System.IO.Path.GetFileName(path)}";
            _log.Dev($"Backup created: {path}");
        }
        else
        {
            _log.Warn("Backup creation failed");
        }
    }

    private async Task<(int ok, int fail)> RunRemovalLoopAsync(List<ItemViewModel> toRemove, bool dryRun)
    {
        var scanner = new RegistryScanner();
        int ok = 0, fail = 0;

        for (int i = 0; i < toRemove.Count; i++)
        {
            var vm   = toRemove[i];
            var item = vm.Item;
            UpdateOverlaySub($"{i + 1} / {toRemove.Count} — {item.Name}");

            if (dryRun)
            {
                _log.Dev($"[DRY-RUN] Would remove: {item.Name}");
                ok++;
                continue;
            }

            if (await Task.Run(() => scanner.RemoveItem(item)))
            {
                _log.Dev($"Removed: {item.Name}");
                ok++;
                await FadeOutRow(vm);
            }
            else
            {
                _log.Error($"Failed to remove: {item.Name}");
                fail++;
            }
        }

        return (ok, fail);
    }

    private async Task FinishRemovalAsync()
    {
        ShowOverlay("Restarting Windows Explorer...", "Applying changes");
        _log.Dev("Restarting Explorer");
        await Task.Run(() => new RegistryScanner().RestartExplorer());
        _log.Dev("Explorer restarted");

        var removed = _viewModels.Where(v => v.RowOpacity < 0.1).ToList();
        foreach (var vm in removed) _viewModels.Remove(vm);
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _viewModels;
        EmptyState.Visibility = _viewModels.Count == 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Animations ───────────────────────────────────────────────────────────

    private Task FadeOutRow(ItemViewModel vm)
    {
        var tcs  = new TaskCompletionSource<bool>();
        var anim = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(280)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        anim.Completed += (_, _) =>
        {
            vm.RowOpacity = 0;
            tcs.TrySetResult(true);
        };
        // Animate via RowOpacity binding
        Dispatcher.Invoke(() =>
        {
            var storyboard = new Storyboard();
            Storyboard.SetTarget(anim, null); // driven via property
            vm.RowOpacity = 1;
            // Directly animate the property
            var timer = new System.Windows.Threading.DispatcherTimer
                { Interval = TimeSpan.FromMilliseconds(10) };
            double elapsed = 0;
            timer.Tick += (_, _) =>
            {
                elapsed += 10;
                double t = Math.Min(elapsed / 280.0, 1.0);
                // Cubic ease-out: 1 - (1-t)^3
                double ease = 1 - Math.Pow(1 - t, 3);
                vm.RowOpacity = 1.0 - ease;
                if (elapsed >= 280)
                {
                    timer.Stop();
                    vm.RowOpacity = 0;
                    tcs.TrySetResult(true);
                }
            };
            timer.Start();
        });
        return tcs.Task;
    }

    // ── Progress helpers ─────────────────────────────────────────────────────

    private void ShowScanProgress(bool visible)
    {
        ScanProgressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (visible) ScanProgressBar.IsIndeterminate = true;
    }

    private void ShowOverlay(string title, string sub = "")
    {
        TxtProgress.Text      = title;
        TxtProgressSub.Text   = sub;
        ProgressOverlay.Visibility = Visibility.Visible;
    }

    private void UpdateOverlaySub(string sub) =>
        Dispatcher.Invoke(() => TxtProgressSub.Text = sub);

    private void HideOverlay() =>
        ProgressOverlay.Visibility = Visibility.Collapsed;
}
