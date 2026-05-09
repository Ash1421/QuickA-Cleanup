using System.Collections.Concurrent;
using Microsoft.Win32;
using QuickA_Cleanup.Core.Models;

namespace QuickA_Cleanup.Core.Services;

/// <summary>
/// Scans Windows Registry for Quick Access navigation pane items.
/// </summary>
public class RegistryScanner
{
    private const string NameSpacePath =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace";

    public (List<QuickAccessItem> Items, int SkippedCount) ScanForItems()
    {
        var bag = new ConcurrentBag<(int order, QuickAccessItem item)>();
        string[] subKeyNames;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath);
            if (key == null) return (new List<QuickAccessItem>(), 0);
            subKeyNames = key.GetSubKeyNames();
        }
        catch
        {
            return (new List<QuickAccessItem>(), 0);
        }

        int skipped = 0;

        Parallel.ForEach(subKeyNames, (subKeyName, _, index) =>
        {
            if (ItemFilter.IsBlacklisted(subKeyName))
            {
                Interlocked.Increment(ref skipped);
                return;
            }

            string? defaultValue;
            try
            {
                using var root   = Registry.CurrentUser.OpenSubKey(NameSpacePath);
                using var subKey = root?.OpenSubKey(subKeyName);
                defaultValue     = subKey?.GetValue("")?.ToString();
            }
            catch { defaultValue = null; }

            if (ItemFilter.IsCustomFolder(defaultValue))
            {
                Interlocked.Increment(ref skipped);
                return;
            }

            var item = new QuickAccessItem
            {
                Number                = 0,
                Guid                  = subKeyName,
                Name                  = ItemFilter.GetDisplayName(subKeyName, defaultValue),
                OriginalRegistryValue = defaultValue,
                RegistryPath          = $@"{NameSpacePath}\{subKeyName}",
                IsProtected           = false,
            };

            if (ItemFilter.IsKnownItem(subKeyName)) item.Tags.Add("known");
            bag.Add(((int)index, item));
        });

        var sorted = bag.OrderBy(t => t.order).Select(t => t.item).ToList();
        for (int i = 0; i < sorted.Count; i++) sorted[i].Number = i + 1;

        return (sorted, skipped);
    }

    public bool RemoveItem(QuickAccessItem item)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath, writable: true);
            if (key == null) return false;
            key.DeleteSubKeyTree(item.Guid, throwOnMissingSubKey: false);
            return true;
        }
        catch { return false; }
    }

    public void RestartExplorer()
    {
        try
        {
            foreach (var p in System.Diagnostics.Process.GetProcessesByName("explorer"))
            {
                p.Kill();
                p.WaitForExit(3000);
            }
            Thread.Sleep(800);
            System.Diagnostics.Process.Start("explorer.exe");
            Thread.Sleep(1200);
        }
        catch { }
    }
}
