namespace QuickA_Cleanup.Core.Services;

/// <summary>
/// Filters and identifies Quick Access items to determine which should be
/// protected or flagged for removal.
/// </summary>
public static class ItemFilter
{
    private static readonly Dictionary<string, string> KnownItems =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "OneDrive - Personal" },
            { "{04271989-C4D2-5507-C554-ABE25D4BDDBA}", "OneDrive" },
            { "{3936E9E4-D92C-4EEE-A85A-BC16D5EA0819}", "OneDrive for Business" },
            { "{AB9B8736-5A5E-45B7-8D4E-3A4B4CDB9121}", "SharePoint" },
            // Test entries from testpins.reg
            { "{TEST0001-0000-0000-0000-000000000001}", "TEST_PIN_Alpha" },
            { "{TEST0002-0000-0000-0000-000000000002}", "TEST_PIN_Beta" },
            { "{TEST0003-0000-0000-0000-000000000003}", "TEST_PIN_Gamma" },
            { "{TEST0004-0000-0000-0000-000000000004}", "TEST_PIN_Delta" },
            { "{TEST0005-0000-0000-0000-000000000005}", "TEST_PIN_Epsilon" },
            { "{TEST0006-0000-0000-0000-000000000006}", "TEST_PIN_Zeta" },
            { "{TEST0007-0000-0000-0000-000000000007}", "TEST_PIN_Eta" },
            { "{TEST0008-0000-0000-0000-000000000008}", "TEST_PIN_Theta" },
        };

    private static readonly HashSet<string> Blacklist =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "{f874310e-b6b7-47dc-bc84-b9e6b38f5903}",
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}",
            "{d3162b92-9365-467a-956b-92703aca08af}",
            "{088e3905-0323-4b02-9826-5d99428e115f}",
            "{24ad3ad4-a569-4530-98e1-ab02f9417aa6}",
            "{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}",
            "{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}",
        };

    private static readonly string[] CustomFolders =
    {
        "Apps", "Code", "Data", "Virtual Machines", "Obsidian Notes",
        "Notepad", "Docker Containers", "Archives", "Workspaces", "Notes",
        "Projects", "Movies", "Anime", "Games", "Maps",
    };

    public static bool IsBlacklisted(string guid) => Blacklist.Contains(guid);

    public static bool IsKnownItem(string guid) => KnownItems.ContainsKey(guid);

    public static bool IsCustomFolder(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName)) return false;
        return CustomFolders.Any(f =>
            displayName.Contains(f, StringComparison.OrdinalIgnoreCase));
    }

    public static string GetDisplayName(string guid, string? defaultValue)
    {
        if (KnownItems.TryGetValue(guid, out var knownName)) return knownName;
        if (!string.IsNullOrWhiteSpace(defaultValue)) return defaultValue;
        return "Unknown Item";
    }
}
