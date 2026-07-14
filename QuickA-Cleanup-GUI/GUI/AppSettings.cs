using System.IO;
using System.Text.Json;

namespace QuickA_Cleanup.GUI;

public static class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QuickA-Cleanup", "app-settings.json");

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
        }
    }
}
