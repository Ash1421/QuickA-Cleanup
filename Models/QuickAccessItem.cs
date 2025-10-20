namespace QuickA_Cleanup.Models;

/// <summary>
/// Represents a Quick Access item found in the Windows Registry
/// </summary>
public class QuickAccessItem
{
    /// <summary>
    /// Display number for user selection
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// The GUID identifier for this Quick Access item
    /// </summary>
    public string Guid { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name of the item (e.g., "OneDrive", "3D Objects")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Full registry path to this item
    /// </summary>
    public string RegistryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this item is protected from removal
    /// </summary>
    public bool IsProtected { get; set; }
    
    /// <summary>
    /// Reason why this item is protected (if applicable)
    /// </summary>
    public string ProtectionReason { get; set; } = string.Empty;
}