namespace QuickA_Cleanup.Core.Models;

/// <summary>
/// Represents a Quick Access item found in the Windows Registry
/// </summary>
public class QuickAccessItem
{
    /// <summary>Display number for user selection</summary>
    public int Number { get; set; }

    /// <summary>The GUID identifier for this Quick Access item</summary>
    public string Guid { get; set; } = string.Empty;

    /// <summary>Display name of the item (e.g., "OneDrive", "3D Objects")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The raw default value read directly from the registry key (before any
    /// friendly-name substitution). Used when writing backup .reg files.
    /// </summary>
    public string? OriginalRegistryValue { get; set; }

    /// <summary>Full registry path to this item (relative to HKCU)</summary>
    public string RegistryPath { get; set; } = string.Empty;

    /// <summary>Whether this item is protected from removal</summary>
    public bool IsProtected { get; set; }

    /// <summary>Reason why this item is protected (if applicable)</summary>
    public string ProtectionReason { get; set; } = string.Empty;

    /// <summary>
    /// Freeform tags used by the UI layer (e.g. "known", "custom").
    /// Not persisted anywhere — set during scanning only.
    /// </summary>
    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
