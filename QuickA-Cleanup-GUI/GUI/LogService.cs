using System.Collections.ObjectModel;
using System.IO;

namespace QuickA_Cleanup.GUI;

public enum LogLevel { Trace, Dev, Warn, Error }

public class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public LogLevel Level     { get; init; }
    public string   Message   { get; init; } = string.Empty;

    public string LevelLabel => Level switch
    {
        LogLevel.Trace => "TRACE",
        LogLevel.Dev   => "DEV  ",
        LogLevel.Warn  => "WARN ",
        LogLevel.Error => "ERROR",
        _              => "     "
    };

    public string Formatted =>
        $"[{Timestamp:HH:mm:ss}] [{LevelLabel}] {Message}";

    public System.Windows.Media.Brush LevelColour => Level switch
    {
        LogLevel.Trace => new System.Windows.Media.SolidColorBrush(
                              System.Windows.Media.Color.FromRgb(0x8B, 0x8B, 0x9E)),
        LogLevel.Dev   => new System.Windows.Media.SolidColorBrush(
                              System.Windows.Media.Color.FromRgb(0x06, 0xB6, 0xD4)),
        LogLevel.Warn  => new System.Windows.Media.SolidColorBrush(
                              System.Windows.Media.Color.FromRgb(0xF5, 0x9E, 0x0B)),
        LogLevel.Error => new System.Windows.Media.SolidColorBrush(
                              System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44)),
        _              => System.Windows.Media.Brushes.White
    };
}

public class LogService
{
    private static LogService? _instance;
    public  static LogService   Instance => _instance ??= new LogService();

    private readonly string _logPath;
    private readonly object _lock = new();

    public ObservableCollection<LogEntry> Entries { get; } = new();
    public LogLevel MinLevel { get; set; } = LogLevel.Trace;

    private LogService()
    {
        _logPath = Path.Combine(AppContext.BaseDirectory, "QuickA-Cleanup.log");
    }

    public void Log(LogLevel level, string message)
    {
        var entry = new LogEntry { Level = level, Message = message };

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            Entries.Add(entry));

        lock (_lock)
        {
            try { File.AppendAllText(_logPath, entry.Formatted + Environment.NewLine); }
            catch { /* never crash on log write */ }
        }
    }

    public void Trace(string m) => Log(LogLevel.Trace, m);
    public void Dev(string m)   => Log(LogLevel.Dev,   m);
    public void Warn(string m)  => Log(LogLevel.Warn,  m);
    public void Error(string m) => Log(LogLevel.Error, m);

    public IEnumerable<LogEntry> Filtered =>
        Entries.Where(e => e.Level >= MinLevel);

    public void Clear() =>
        System.Windows.Application.Current?.Dispatcher.Invoke(() => Entries.Clear());
}
