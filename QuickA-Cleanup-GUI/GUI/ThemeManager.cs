using System.IO;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace QuickA_Cleanup.GUI;

public enum AppThemeMode { Light, Dark, System }

/// <summary>Whether the accent colour follows a fixed swatch or the live Windows system accent.</summary>
public enum AccentMode { Custom, WindowsAccent }

public class AccentSwatch
{
    public string Name { get; init; } = string.Empty;
    public Color Color { get; init; }
}

/// <summary>
/// Fixed semantic status colours (success / caution / critical) used for the
/// header dot indicators. Deliberately not theme-resource-driven: resolving
/// ThemeResource keys from code-behind is unreliable in WinUI 3, and these
/// specific hues read fine on both light and dark surfaces anyway.
/// </summary>
public static class StatusColors
{
    public static readonly SolidColorBrush Success  = new(Color.FromArgb(255, 0x10, 0xB9, 0x81));
    public static readonly SolidColorBrush Caution  = new(Color.FromArgb(255, 0xF5, 0x9E, 0x0B));
    public static readonly SolidColorBrush Critical = new(Color.FromArgb(255, 0xEF, 0x44, 0x44));
}

/// <summary>
/// Owns the app's theme mode (Light/Dark/System) and accent colour, including
/// persistence to a small JSON file under %LocalAppData%.
///
/// Accent is applied two ways: (1) mutating the app's own "AccentBrush"/
/// "AccentBrushSubtle" SolidColorBrush instances in place, which updates
/// every control referencing them live, with no restart; and (2) overwriting
/// the Application-level SystemAccentColor + tonal variants, so native Fluent
/// controls (CheckBox, RadioButton, etc.) that bind to the *real* Windows
/// accent pick up the same colour instead of the OS's own blue. Note (2) only
/// affects controls created *after* the change (e.g. the next time Settings
/// is opened) — WinUI doesn't retroactively re-flow already-instantiated
/// native control templates the way DynamicResource does in WPF.
/// </summary>
public static class ThemeManager
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QuickA-Cleanup", "ui-settings.json");

    private static readonly UISettings SystemUiSettings = new();
    private static bool _listeningForSystemAccent;

    public static readonly AccentSwatch[] Swatches =
    {
        new() { Name = "Violet",   Color = Color.FromArgb(255, 0x7C, 0x3A, 0xED) },
        new() { Name = "Blue",     Color = Color.FromArgb(255, 0x00, 0x78, 0xD4) },
        new() { Name = "Teal",     Color = Color.FromArgb(255, 0x00, 0x89, 0x8A) },
        new() { Name = "Green",    Color = Color.FromArgb(255, 0x10, 0xB9, 0x81) },
        new() { Name = "Amber",    Color = Color.FromArgb(255, 0xF7, 0x63, 0x0C) },
        new() { Name = "Rose",     Color = Color.FromArgb(255, 0xE3, 0x00, 0x8C) },
        new() { Name = "Red",      Color = Color.FromArgb(255, 0xC4, 0x2B, 0x1C) },
        new() { Name = "Graphite", Color = Color.FromArgb(255, 0x5A, 0x5A, 0x5E) },
    };

    public static AppThemeMode ThemeMode { get; private set; } = AppThemeMode.System;
    public static AccentMode AccentSource { get; private set; } = AccentMode.Custom;
    public static Color AccentColor { get; private set; } = Swatches[0].Color; // Violet by default

    private class Persisted
    {
        public string Theme { get; set; } = "System";
        public string AccentSource { get; set; } = "Custom";
        public string Accent { get; set; } = "#7C3AED";
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var data = JsonSerializer.Deserialize<Persisted>(File.ReadAllText(SettingsPath));
                if (data != null)
                {
                    ThemeMode    = Enum.TryParse<AppThemeMode>(data.Theme, out var mode) ? mode : AppThemeMode.System;
                    AccentSource = Enum.TryParse<AccentMode>(data.AccentSource, out var src) ? src : AccentMode.Custom;
                    AccentColor  = AccentSource == AccentMode.WindowsAccent
                        ? GetWindowsAccentColor()
                        : ParseHex(data.Accent) ?? Swatches[0].Color;
                }
            }
        }
        catch
        {
            // Fall back to defaults — never block startup on a corrupt settings file.
        }

        if (AccentSource == AccentMode.WindowsAccent)
            ListenForSystemAccentChanges();
    }

    public static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);

            var data = new Persisted
            {
                Theme        = ThemeMode.ToString(),
                AccentSource = AccentSource.ToString(),
                Accent       = $"#{AccentColor.R:X2}{AccentColor.G:X2}{AccentColor.B:X2}"
            };

            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(data));
        }
        catch
        {
            // Never crash the app over a settings write failure.
        }
    }

    public static void ApplyTheme(FrameworkElement root, AppThemeMode mode)
    {
        ThemeMode = mode;
        root.RequestedTheme = mode switch
        {
            AppThemeMode.Light => ElementTheme.Light,
            AppThemeMode.Dark  => ElementTheme.Dark,
            _                  => ElementTheme.Default
        };
        Save();
    }

    /// <summary>Pin the accent to a fixed swatch colour.</summary>
    public static void ApplyAccent(Color color)
    {
        AccentSource = AccentMode.Custom;
        StopListeningForSystemAccent();
        AccentColor = color;
        MutateBrushes(color);
        Save();
    }

    /// <summary>Follow the live Windows system accent colour instead of a fixed swatch.</summary>
    public static void ApplyWindowsAccent()
    {
        AccentSource = AccentMode.WindowsAccent;
        AccentColor = GetWindowsAccentColor();
        MutateBrushes(AccentColor);
        ListenForSystemAccentChanges();
        Save();
    }

    private static void ListenForSystemAccentChanges()
    {
        if (_listeningForSystemAccent) return;
        _listeningForSystemAccent = true;
        SystemUiSettings.ColorValuesChanged += OnSystemColorsChanged;
    }

    private static void StopListeningForSystemAccent()
    {
        if (!_listeningForSystemAccent) return;
        _listeningForSystemAccent = false;
        SystemUiSettings.ColorValuesChanged -= OnSystemColorsChanged;
    }

    private static void OnSystemColorsChanged(UISettings sender, object args)
    {
        if (AccentSource != AccentMode.WindowsAccent) return;

        AccentColor = GetWindowsAccentColor();

        // ColorValuesChanged fires off the UI thread — marshal back before touching brushes.
        App.MainWin?.DispatcherQueue.TryEnqueue(() => MutateBrushes(AccentColor));
    }

    private static Color GetWindowsAccentColor() =>
        SystemUiSettings.GetColorValue(UIColorType.Accent);

    private static void MutateBrushes(Color color)
    {
        var app = Application.Current;
        if (app == null) return;

        MutateBrush(app, "AccentBrush", color);
        MutateBrush(app, "AccentBrushSubtle", color);

        // The keys native Fluent controls (CheckBox, RadioButton, ToggleSwitch, ...) actually
        // bind to for their checked/on-state fill — this is the real fix for those controls
        // still showing Windows' own accent colour.
        MutateBrush(app, "AccentFillColorDefaultBrush", color);
        MutateBrush(app, "AccentFillColorSecondaryBrush", color);
        MutateBrush(app, "AccentFillColorTertiaryBrush", color);

        // Belt-and-braces: also override the raw system-accent resources, in case any
        // control paths reference SystemAccentColor dynamically rather than via a
        // pre-baked brush.
        app.Resources["SystemAccentColor"]      = color;
        app.Resources["SystemAccentColorLight1"] = Blend(color, 0.16);
        app.Resources["SystemAccentColorLight2"] = Blend(color, 0.32);
        app.Resources["SystemAccentColorLight3"] = Blend(color, 0.48);
        app.Resources["SystemAccentColorDark1"]  = Blend(color, -0.16);
        app.Resources["SystemAccentColorDark2"]  = Blend(color, -0.32);
        app.Resources["SystemAccentColorDark3"]  = Blend(color, -0.48);
    }

    private static void MutateBrush(Application app, string key, Color color)
    {
        if (app.Resources.TryGetValue(key, out var obj) && obj is SolidColorBrush brush)
            brush.Color = color;
    }

    /// <summary>Blends towards white (positive amount) or black (negative amount).</summary>
    private static Color Blend(Color c, double amount)
    {
        byte target = amount >= 0 ? (byte)255 : (byte)0;
        double t = Math.Abs(amount);

        byte Mix(byte channel) => (byte)Math.Clamp(channel + ((target - channel) * t), 0, 255);

        return Color.FromArgb(255, Mix(c.R), Mix(c.G), Mix(c.B));
    }

    private static Color? ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        hex = hex.TrimStart('#');
        if (hex.Length != 6) return null;

        try
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }
        catch
        {
            return null;
        }
    }
}
