using Microsoft.Win32;
using QuickA_Cleanup.Models;

namespace QuickA_Cleanup.Services;

/// <summary>
/// Scans Windows Registry for Quick Access navigation pane items
/// </summary>
public class RegistryScanner
{
    private const string NameSpacePath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace";

    /// <summary>
    /// Scans the registry for Quick Access items
    /// </summary>
    /// <returns>List of found Quick Access items (excluding protected items)</returns>
    public List<QuickAccessItem> ScanForItems()
    {
        var items = new List<QuickAccessItem>();
        var itemNumber = 1;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath);
            if (key == null)
            {
                Console.WriteLine("Registry path not found.");
                return items;
            }

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                var guid = subKeyName;

                // Skip blacklisted items (essential system folders)
                if (ItemFilter.IsBlacklisted(guid))
                    continue;

                using var subKey = key.OpenSubKey(subKeyName);
                var defaultValue = subKey?.GetValue("")?.ToString();

                // Skip custom folders (user-defined folders)
                if (ItemFilter.IsCustomFolder(defaultValue))
                    continue;

                var displayName = ItemFilter.GetDisplayName(guid, defaultValue);

                items.Add(new QuickAccessItem
                {
                    Number = itemNumber++,
                    Guid = guid,
                    Name = displayName,
                    RegistryPath = $@"{NameSpacePath}\{subKeyName}",
                    IsProtected = false
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning registry: {ex.Message}");
        }

        return items;
    }

    /// <summary>
    /// Removes a Quick Access item from the registry
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool RemoveItem(QuickAccessItem item)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(NameSpacePath, writable: true);
            if (key == null)
                return false;

            key.DeleteSubKeyTree(item.Guid, throwOnMissingSubKey: false);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing {item.Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Restarts Windows Explorer to apply changes
    /// </summary>
    public void RestartExplorer()
    {
        try
        {
            var explorerProcesses = System.Diagnostics.Process.GetProcessesByName("explorer");
            foreach (var process in explorerProcesses)
            {
                process.Kill();
                process.WaitForExit();
            }

            // Wait a moment before restarting
            System.Threading.Thread.Sleep(1000);
            
            // Start Explorer again
            System.Diagnostics.Process.Start("explorer.exe");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restarting Explorer: {ex.Message}");
            Console.WriteLine("Please restart Explorer manually (Ctrl+Shift+Esc → File → Run new task → explorer.exe)");
        }
    }
}