using Microsoft.Win32;

namespace QuickA_Cleanup.Core.Services;

/// <summary>
/// Manages the testpins.reg dummy registry entries used for testing.
/// </summary>
public static class TestpinService
{
    private const string NameSpacePath =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace";

    private static readonly string[] TestGuids =
    {
        "{TEST0001-0000-0000-0000-000000000001}",
        "{TEST0002-0000-0000-0000-000000000002}",
        "{TEST0003-0000-0000-0000-000000000003}",
        "{TEST0004-0000-0000-0000-000000000004}",
        "{TEST0005-0000-0000-0000-000000000005}",
        "{TEST0006-0000-0000-0000-000000000006}",
        "{TEST0007-0000-0000-0000-000000000007}",
        "{TEST0008-0000-0000-0000-000000000008}",
    };

    /// <summary>
    /// Returns true if any testpin GUIDs are present in the registry.
    /// </summary>
    public static bool AreInstalled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath);
            if (key == null) return false;
            var subKeys = key.GetSubKeyNames();
            return TestGuids.Any(g =>
                subKeys.Contains(g, StringComparer.OrdinalIgnoreCase));
        }
        catch { return false; }
    }

    /// <summary>
    /// Returns how many testpin GUIDs are currently installed.
    /// </summary>
    public static int InstalledCount()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath);
            if (key == null) return 0;
            var subKeys = key.GetSubKeyNames();
            return TestGuids.Count(g =>
                subKeys.Contains(g, StringComparer.OrdinalIgnoreCase));
        }
        catch { return 0; }
    }

    /// <summary>
    /// Removes all testpin entries from the registry.
    /// Returns the number successfully removed.
    /// </summary>
    public static int RemoveAll()
    {
        int removed = 0;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath, writable: true);
            if (key == null) return 0;

            foreach (var guid in TestGuids)
            {
                try
                {
                    key.DeleteSubKeyTree(guid, throwOnMissingSubKey: false);
                    removed++;
                }
                catch { /* skip any that fail */ }
            }
        }
        catch { }
        return removed;
    }
}
