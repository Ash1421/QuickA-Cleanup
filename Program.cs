using QuickA_Cleanup.Models;
using QuickA_Cleanup.Services;

namespace QuickA_Cleanup;

/// <summary>
/// Main entry point for QuickA-Cleanup
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.Title = "QuickA-Cleanup";
        
        try
        {
            RunCleanup();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Main cleanup workflow
    /// </summary>
    static void RunCleanup()
    {
        Console.Clear();
        PrintHeader();

        var scanner = new RegistryScanner();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Starting scan...");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Scanning for Quick Access items...");
        Console.ResetColor();
        Console.WriteLine();

        var foundItems = scanner.ScanForItems();

        if (foundItems.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("No Quick Access navigation items found in registry.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            return;
        }

        DisplayFoundItems(foundItems);
        
        var itemsToRemove = GetUserSelection(foundItems);

        if (itemsToRemove.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No valid selection made.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            return;
        }

        if (!ConfirmRemoval(itemsToRemove))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Cancelled.");
            Console.ResetColor();
            return;
        }

        RemoveItems(scanner, itemsToRemove);
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done! Restarting Windows Explorer...");
        Console.ResetColor();

        scanner.RestartExplorer();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Complete! Check File Explorer navigation pane.");
        Console.ResetColor();
        Console.WriteLine();
        
        System.Threading.Thread.Sleep(2000);
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Prints the application header
    /// </summary>
    static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Quick Access Navigation Pane Item Remover ===");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Displays the list of found items in a table format
    /// </summary>
    static void DisplayFoundItems(List<QuickAccessItem> items)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Found the following Quick Access items:");
        Console.ResetColor();
        Console.WriteLine();

        // Table header
        Console.WriteLine($"{"Number",-8} {"GUID",-40} {"Name"}");
        Console.WriteLine(new string('-', 80));

        // Table rows
        foreach (var item in items)
        {
            Console.WriteLine($"{item.Number,-8} {item.Guid,-40} {item.Name}");
        }
    }

    /// <summary>
    /// Gets the user's selection of items to remove
    /// </summary>
    static List<QuickAccessItem> GetUserSelection(List<QuickAccessItem> items)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Options:");
        Console.ResetColor();
        Console.WriteLine("  [Number] - Remove specific item by number");
        Console.WriteLine("  [A]      - Remove ALL items");
        Console.WriteLine("  [Q]      - Quit without changes");
        Console.WriteLine();

        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine()?.Trim() ?? "";

        // Quit option
        if (choice.Equals("Q", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Exiting without changes.");
            Console.ResetColor();
            Environment.Exit(0);
        }

        // Remove all option
        if (choice.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            return items;
        }

        // Parse specific numbers
        var selectedItems = new List<QuickAccessItem>();
        var numbers = choice.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var numStr in numbers)
        {
            if (int.TryParse(numStr.Trim(), out int num))
            {
                var item = items.FirstOrDefault(i => i.Number == num);
                if (item != null)
                {
                    selectedItems.Add(item);
                }
            }
        }

        return selectedItems;
    }

    /// <summary>
    /// Confirms removal of selected items with the user
    /// </summary>
    static bool ConfirmRemoval(List<QuickAccessItem> items)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("You are about to remove:");
        Console.ResetColor();

        foreach (var item in items)
        {
            Console.WriteLine($"  - {item.Name} ({item.Guid})");
        }

        Console.WriteLine();
        Console.Write("Are you sure? (Y/N): ");
        var confirm = Console.ReadLine()?.Trim() ?? "";

        return confirm.Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes the selected items from the registry
    /// </summary>
    static void RemoveItems(RegistryScanner scanner, List<QuickAccessItem> items)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Removing items...");
        Console.ResetColor();

        foreach (var item in items)
        {
            if (scanner.RemoveItem(item))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Removed: {item.Name}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ FAILED to remove: {item.Name}");
                Console.ResetColor();
            }
        }
    }
}