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
        if (!ElevationHelper.IsElevated())
        {
            ElevationHelper.TryRelaunchElevated();
            Environment.Exit(0);
            return;
        }

        ThemeManager.Load();
        AppSettings.Load();

        MainWin = new MainWindow();
        MainWin.Activate();

        UpdateService.CheckSilentlyOnStartup();
    }
}
