using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuickA_Cleanup.Core.Models;
using QuickA_Cleanup.Core.Services;
using WinUIEx;

namespace QuickA_Cleanup.GUI;

public sealed partial class MainWindow : WindowEx
{
    /// <summary>Pause between steps during removal/dry-run so progress is visible
    /// instead of flashing past instantly — registry ops are near-instant otherwise.</summary>
    private const int StepDelayMs = 220;

    public ObservableCollection<ItemViewModel> Items { get; } = new();

    private readonly LogService _log = LogService.Instance;

    public MainWindow()
    {
        InitializeComponent();

        SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var version = GetType().Assembly.GetName().Version;
        VersionBadge.Text = $"V{version?.ToString(3) ?? "3.0.0"}";

        ThemeManager.ApplyTheme((FrameworkElement)Content, ThemeManager.ThemeMode);
        ThemeManager.ApplyAccent(ThemeManager.AccentColor);

        DotBloat.Fill = StatusColors.Success;
        DotTestpin.Fill = StatusColors.Caution;
        DotError.Fill = StatusColors.Success;

        _log.Trace("MainWindow initialised");
        CheckTestpinsOnStartup();
    }

    // ── Status dots ──────────────────────────────────────────────────────────

    private void UpdateStatusDots(bool? hasBloat = null)
    {
        bool hasErrors = _log.Entries.Any(e => e.Level == LogLevel.Error);
        DotError.Fill = hasErrors ? StatusColors.Critical : StatusColors.Success;

        if (hasBloat.HasValue)
            DotBloat.Fill = hasBloat.Value ? StatusColors.Caution : StatusColors.Success;

        DotTestpinGroup.Visibility = TestpinService.AreInstalled() ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CheckTestpinsOnStartup()
    {
        int count = TestpinService.InstalledCount();
        TxtStatus.Text = count > 0
            ? $"{count} test entry/entries detected — open Settings to remove."
            : "Ready — click Scan to begin.";

        if (count > 0) _log.Warn($"Testpins detected — {count} test entry/entries installed.");
        UpdateStatusDots();
    }

    // ── Settings ─────────────────────────────────────────────────────────────

    private async void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        _log.Trace("Settings opened");
        var dialog = new SettingsDialog(this) { XamlRoot = Content.XamlRoot };
        await dialog.ShowAsync();
        UpdateStatusDots();
    }

    // ── Scan ─────────────────────────────────────────────────────────────────

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        ShowOverlay("Scanning registry...", "Looking for Quick Access entries");
        ScanProgressBar.Visibility = Visibility.Visible;
        BtnScan.IsEnabled = false;
        _log.Dev("Registry scan started");

        try
        {
            var scanner = new RegistryScanner();
            var (found, skipped) = await Task.Run(() => scanner.ScanForItems());

            foreach (var item in found)
                if (item.Name.StartsWith("TEST_PIN", StringComparison.OrdinalIgnoreCase))
                    item.Tags.Add("testpin");

            Items.Clear();
            foreach (var item in found)
            {
                var vm = new ItemViewModel(item);
                vm.SelectionChanged += UpdateSelectionCount;
                Items.Add(vm);
            }

            EmptyState.Visibility = Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            bool hasBloat = found.Any(i => i.Tags.Contains("known") && !i.Tags.Contains("testpin"));
            TxtStatus.Text = Items.Count == 0
                ? "No removable items found."
                : $"{Items.Count} item(s) found  •  {skipped} protected hidden" + (hasBloat ? "  •  bloatware detected" : "");

            UpdateStatusDots(hasBloat);
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
            ScanProgressBar.Visibility = Visibility.Collapsed;
            BtnScan.IsEnabled = true;
        }
    }

    // ── Selection ────────────────────────────────────────────────────────────

    private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var vm in Items) vm.IsSelected = true;
        UpdateSelectionCount();
    }

    private void BtnDeselectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var vm in Items) vm.IsSelected = false;
        UpdateSelectionCount();
    }

    private void UpdateSelectionCount()
    {
        int count = Items.Count(v => v.IsSelected);
        TxtSelectionCount.Text = $"{count} selected";
        BtnRemove.IsEnabled = count > 0;
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    private async void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        var toRemove = Items.Where(v => v.IsSelected).ToList();
        bool dryRun = ChkDryRun.IsChecked == true;
        bool backup = ChkBackup.IsChecked == true;

        var confirm = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Confirm",
            Content = $"Are you sure you want to {(dryRun ? "simulate removal of" : "remove")} {toRemove.Count} item(s)?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };
        if (await confirm.ShowAsync() != ContentDialogResult.Primary)
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

            if (!dryRun && AppSettings.RestartExplorerAfterRemoval)
            {
                ShowOverlay("Restarting Windows Explorer...", "Applying changes");
                await Task.Run(() => new RegistryScanner().RestartExplorer());
                await Task.Delay(StepDelayMs);
                EmptyState.Visibility = Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (!dryRun)
            {
                EmptyState.Visibility = Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            string msg = dryRun
                ? $"[Dry Run] Would have removed {ok} item(s)."
                : $"Done — {ok} removed, {fail} failed.";

            _log.Dev(msg);
            TxtStatus.Text = msg;
            UpdateStatusDots();

            var result = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Complete",
                Content = msg,
                CloseButtonText = "OK"
            };
            await result.ShowAsync();
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
            var vm = toRemove[i];
            var item = vm.Item;
            UpdateOverlaySub($"{i + 1} / {toRemove.Count} — {item.Name}");
            await Task.Delay(StepDelayMs); // let the step be readable before/while it runs

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
                Items.Remove(vm); // ListView animates the row out automatically
            }
            else
            {
                _log.Error($"Failed to remove: {item.Name}");
                fail++;
            }
        }

        return (ok, fail);
    }

    // ── Progress helpers ─────────────────────────────────────────────────────

    private void ShowOverlay(string title, string sub = "")
    {
        TxtProgress.Text = title;
        TxtProgressSub.Text = sub;
        ProgressOverlay.Visibility = Visibility.Visible;
    }

    private void UpdateOverlaySub(string sub) =>
        DispatcherQueue.TryEnqueue(() => TxtProgressSub.Text = sub);

    private void HideOverlay() => ProgressOverlay.Visibility = Visibility.Collapsed;
}
