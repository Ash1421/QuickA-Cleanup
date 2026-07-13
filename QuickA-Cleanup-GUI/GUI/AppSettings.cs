using System.IO;
using System.Text.Json;

namespace QuickA_Cleanup.GUI;

/// <summary>
/// Small persisted, non-appearance app settings (things that live under a
/// "Developer" section rather than "Appearance"). Kept separate from
/// ThemeManager since these are behavioural toggles, not theming.
/// </summary>
public static class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QuickA-Cleanup", "app-settings.json");

    /// <summary>
    /// Whether Explorer is restarted after a real (non-dry-run) removal.
    /// Enabled by default — turning this off means removed entries won't
    /// disappear from the navigation pane until Explorer is restarted some
    /// other way, so it's flagged as not recommended in the UI.
    /// </summary>
    public static bool RestartExplorerAfterRemoval { get; set; } = true;

    private class Persisted
    {
        public bool RestartExplorerAfterRemoval { get; set; } = true;
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var data = JsonSerializer.Deserialize<Persisted>(File.ReadAllText(SettingsPath));
                if (data != null)
                    RestartExplorerAfterRemoval = data.RestartExplorerAfterRemoval;
            }
        }
        catch
        {
            // Fall back to defaults — never block startup on a corrupt settings file.
        }
    }

    public static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);

            var data = new Persisted { RestartExplorerAfterRemoval = RestartExplorerAfterRemoval };
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(data));
        }
        catch
        {
            // Never crash the app over a settings write failure.
        }
    }
}
