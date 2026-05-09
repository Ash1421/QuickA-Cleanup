using QuickA_Cleanup.Core.Models;
using QuickA_Cleanup.Core.Services;

namespace QuickA_Cleanup.CLI;

class Program
{

    static void Main(string[] args)
    {
        Console.Title = "QuickA-Cleanup CLI";
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        bool dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        bool noBackup = args.Contains("--no-backup", StringComparer.OrdinalIgnoreCase);
        bool help = args.Contains("--help", StringComparer.OrdinalIgnoreCase)
                    || args.Contains("-h", StringComparer.OrdinalIgnoreCase);

        if (help) { PrintHelp(); return; }

        try { RunCleanup(dryRun, !noBackup); }
        catch (Exception ex)
        {
            Error($"\nFATAL: {ex.Message}");
            Pause();
            Environment.Exit(1);
        }
    }

    static void RunCleanup(bool dryRun, bool backupEnabled)
    {
        Console.Clear();
        PrintHeader();

        if (dryRun)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  [DRY-RUN] No changes will be written.\n");
            Console.ResetColor();
        }

        var scanner = new RegistryScanner();
        Status("Scanning registry for Quick Access items...");

        var (items, skipped) = scanner.ScanForItems();
        Console.WriteLine();

        if (items.Count == 0)
        {
            Success("No removable items found." +
                    (skipped > 0 ? $" ({skipped} protected item(s) hidden.)" : ""));
            Pause();
            return;
        }

        PrintTable(items, skipped);

        var toRemove = GetSelection(items);

        if (toRemove.Count == 0)
        {
            Warn("No valid selection — nothing to do.");
            Pause();
            return;
        }

        if (!Confirm(toRemove, dryRun)) { Warn("Cancelled."); return; }

        if (backupEnabled && !dryRun)
        {
            var exeDir = AppContext.BaseDirectory;
            var path = BackupService.CreateBackup(toRemove, exeDir);
            if (path != null) Success($"Backup → {System.IO.Path.GetFileName(path)}");
            else Warn("Backup failed — continuing anyway.");
            Console.WriteLine();
        }

        RemoveItems(scanner, toRemove, dryRun);

        if (!dryRun)
        {
            Console.WriteLine();
            Status("Restarting Windows Explorer...");
            scanner.RestartExplorer();
            Success("Done! Check your File Explorer navigation pane.");
        }
        else
        {
            Success("Dry-run complete. No changes were made.");
        }

        Console.WriteLine();
        Thread.Sleep(1500);
        Pause();
    }

    // ── Selection ────────────────────────────────────────────────────────────

    static List<QuickAccessItem> GetSelection(List<QuickAccessItem> items)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Options:");
        Console.ResetColor();
        Console.WriteLine("    [number]   Remove one item        (e.g. 2)");
        Console.WriteLine("    [n,n,n]    Remove multiple items  (e.g. 1,3,5)");
        Console.WriteLine("    [A]        Remove ALL items");
        Console.WriteLine("    [Q]        Quit without changes");
        Console.WriteLine();
        Console.Write("  Choice: ");

        var input = Console.ReadLine()?.Trim() ?? "";

        if (input.Equals("Q", StringComparison.OrdinalIgnoreCase))
        {
            Warn("Exiting without changes.");
            Environment.Exit(0);
        }

        if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
            return new List<QuickAccessItem>(items);

        var selected = new List<QuickAccessItem>();
        foreach (var part in input.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part.Trim(), out int num))
            {
                var item = items.FirstOrDefault(i => i.Number == num);
                if (item != null && !selected.Contains(item))
                    selected.Add(item);
            }
        }

        if (selected.Count == 0) Warn("No matching numbers found.");
        return selected;
    }

    static bool Confirm(List<QuickAccessItem> items, bool dryRun)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(dryRun ? "  [DRY-RUN] Would remove:" : "  About to remove:");
        Console.ResetColor();
        foreach (var i in items)
            Console.WriteLine($"    • {i.Name,-35} {i.Guid}");
        Console.WriteLine();
        Console.Write(dryRun ? "  Simulate? (Y/N): " : "  Confirm? (Y/N): ");
        return (Console.ReadLine()?.Trim() ?? "").Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    static void RemoveItems(RegistryScanner scanner, List<QuickAccessItem> items, bool dryRun)
    {
        Console.WriteLine();
        int ok = 0, fail = 0;

        foreach (var item in items)
        {
            if (dryRun)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"    [DRY-RUN] Would remove: {item.Name}");
                Console.ResetColor();
                ok++;
                continue;
            }

            if (scanner.RemoveItem(item))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    ✓ Removed : {item.Name}");
                Console.ResetColor();
                ok++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    ✗ Failed  : {item.Name}");
                Console.ResetColor();
                fail++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"  Result: {ok} succeeded, {fail} failed.");
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    static void PrintHeader()
    {
        const int w = 60;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', w));
        Console.WriteLine(Center("QuickA-Cleanup CLI", w));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(Center("Quick Access Navigation Pane Cleaner", w));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', w));
        Console.ResetColor();
        Console.WriteLine();
    }

    static void PrintHelp()
    {
        PrintHeader();
        Console.WriteLine("  USAGE:  QuickA-Cleanup-CLI.exe [options]\n");
        Console.WriteLine("  OPTIONS:");
        Console.WriteLine("    --dry-run      Preview changes without writing to registry");
        Console.WriteLine("    --no-backup    Skip automatic .reg backup before removal");
        Console.WriteLine("    --help / -h    Show this help\n");
    }

    static void PrintTable(List<QuickAccessItem> items, int skipped)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Found {items.Count} item(s)" +
                        (skipped > 0 ? $"  ({skipped} protected hidden)" : "") + "\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {"#",-5} {"Name",-35} {"GUID",-40} {"Type"}");
        Console.WriteLine("  " + new string('─', 88));
        Console.ResetColor();

        foreach (var item in items)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"  {item.Number,-5}");

            Console.ForegroundColor = item.Tags.Contains("known")
                ? ConsoleColor.Yellow : ConsoleColor.White;
            Console.Write($"{Truncate(item.Name, 34),-35} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{item.Guid,-40} ");

            Console.ForegroundColor = item.Tags.Contains("known")
                ? ConsoleColor.DarkYellow : ConsoleColor.DarkGray;
            Console.WriteLine(item.Tags.Contains("known") ? "bloatware" : "unknown");

            Console.ResetColor();
        }

        Console.WriteLine();
    }

    static void Status(string m) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("  ► "); Console.ResetColor(); Console.WriteLine(m); }
    static void Success(string m) { Console.ForegroundColor = ConsoleColor.Green; Console.Write("  ✓ "); Console.ResetColor(); Console.WriteLine(m); }
    static void Warn(string m) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("  ⚠ "); Console.ResetColor(); Console.WriteLine(m); }
    static void Error(string m) { Console.ForegroundColor = ConsoleColor.Red; Console.Write("  ✗ "); Console.ResetColor(); Console.WriteLine(m); }
    static void Pause() { Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("\n  Press Enter to exit..."); Console.ResetColor(); Console.ReadLine(); }

    static string Center(string text, int width)
    {
        int pad = Math.Max(0, (width - text.Length) / 2);
        return new string(' ', pad) + text;
    }

    static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..(max - 1)] + "…";
}
