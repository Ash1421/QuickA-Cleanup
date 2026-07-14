using Microsoft.UI.Xaml;
using Velopack;

namespace QuickA_Cleanup.GUI;

public partial class App : Application
{
    public static MainWindow? MainWin { get; private set; }

    public App()
    {
        VelopackApp.Build().Run();

        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ThemeManager.Load();
        AppSettings.Load();

        MainWin = new MainWindow();
        MainWin.Activate();

        UpdateService.CheckSilentlyOnStartup();
    }
}
