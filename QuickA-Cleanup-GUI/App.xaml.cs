using Microsoft.UI.Xaml;

namespace QuickA_Cleanup.GUI;

/// <summary>
/// Application entry point. Loads persisted UI settings (theme + accent colour)
/// before the window is created so there's no flash of default styling.
/// </summary>
public partial class App : Application
{
    public static MainWindow? MainWin { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ThemeManager.Load();
        AppSettings.Load();

        MainWin = new MainWindow();
        MainWin.Activate();
    }
}
