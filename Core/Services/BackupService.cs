using System.IO;
using Microsoft.Win32;
using QuickA_Cleanup.Core.Models;

namespace QuickA_Cleanup.Core.Services;

/// <summary>
/// Creates .reg backup files before any registry deletion.
/// </summary>
public static class BackupService
{
    public static string? CreateBackup(IEnumerable<QuickAccessItem> items, string outputDirectory)
    {
        try
        {
            var timestamp  = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var backupPath = Path.Combine(outputDirectory, $"QuickA-Backup-{timestamp}.reg");

            const string basePath =
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace";

            var lines = new List<string>
            {
                "Windows Registry Editor Version 5.00",
                $"; QuickA-Cleanup backup — {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                "; Restore by double-clicking this file.",
                ""
            };

            foreach (var item in items)
            {
                lines.Add($"[{basePath}\\{item.Guid}]");
                lines.Add($"@=\"{EscapeRegValue(item.OriginalRegistryValue ?? item.Name)}\"");
                lines.Add("");
            }

            File.WriteAllLines(backupPath, lines, System.Text.Encoding.Unicode);
            return backupPath;
        }
        catch
        {
            return null;
        }
    }

    private static string EscapeRegValue(string value) =>
        value.Replace(@"\", @"\\").Replace("\"", "\\\"");
}
