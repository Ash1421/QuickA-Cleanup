namespace QuickA_Cleanup.Services;

/// <summary>
/// Filters and identifies Quick Access items to determine which should be protected or removed
/// </summary>
public static class ItemFilter
{
    /// <summary>
    /// Known bloatware GUIDs and their display names
    /// </summary>
    private static readonly Dictionary<string, string> KnownItems = new()
    {
        { "{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "OneDrive - Personal" },
        { "{04271989-C4D2-5507-C554-ABE25D4BDDBA}", "OneDrive" }
    };

    /// <summary>
    /// System folders that should NEVER be removed (essential Windows folders)
    /// </summary>
    private static readonly HashSet<string> Blacklist = new(StringComparer.OrdinalIgnoreCase)
    {
        "{f874310e-b6b7-47dc-bc84-b9e6b38f5903}",  // Pictures
        "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}",  // Desktop
        "{d3162b92-9365-467a-956b-92703aca08af}",  // Documents
        "{088e3905-0323-4b02-9826-5d99428e115f}",  // Downloads
        "{24ad3ad4-a569-4530-98e1-ab02f9417aa6}",  // Pictures (alternate)
        "{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}",  // Music
        "{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}"   // Videos
    };

    /// <summary>
    /// User's custom folders to protect (add your own folder names here)
    /// </summary>
    private static readonly string[] CustomFolders = 
    {
        "Apps",
        "Code",
        "Data",
        "Virtual Machines",
        "Obsidian Notes",
        "Notepad",
        "Docker Containers",
        "Archives",
        "Workspaces"
    };

    /// <summary>
    /// Checks if a GUID is in the protected blacklist
    /// </summary>
    /// <param name="guid">The GUID to check</param>
    /// <returns>True if the GUID is blacklisted (protected)</returns>
    public static bool IsBlacklisted(string guid)
    {
        return Blacklist.Contains(guid);
    }

    /// <summary>
    /// Checks if a display name matches a custom folder pattern
    /// </summary>
    /// <param name="displayName">The display name to check</param>
    /// <returns>True if this is a custom folder that should be protected</returns>
    public static bool IsCustomFolder(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return false;

        return CustomFolders.Any(folder => 
            displayName.Contains(folder, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a friendly display name for a GUID
    /// </summary>
    /// <param name="guid">The GUID to lookup</param>
    /// <param name="defaultValue">The default registry value (if any)</param>
    /// <returns>A friendly display name</returns>
    public static string GetDisplayName(string guid, string? defaultValue)
    {
        // Check if we have a known name for this GUID
        if (KnownItems.TryGetValue(guid, out var knownName))
            return knownName;

        // Use the registry default value if available
        if (!string.IsNullOrWhiteSpace(defaultValue))
            return defaultValue;

        // Fallback to unknown
        return "Unknown Item";
    }
}