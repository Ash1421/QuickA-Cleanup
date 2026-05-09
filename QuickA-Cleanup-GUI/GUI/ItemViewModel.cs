using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using QuickA_Cleanup.Core.Models;

namespace QuickA_Cleanup.GUI;

public class ItemViewModel : INotifyPropertyChanged
{
    public QuickAccessItem Item { get; }

    public ItemViewModel(QuickAccessItem item) => Item = item;

    public string Name => Item.Name;
    public string Guid => Item.Guid;

    // ── Tag badge ────────────────────────────────────────────────────────────

    public string TagLabel =>
        Item.Tags.Contains("known")    ? "Bloatware" :
        Item.Tags.Contains("testpin")  ? "Test"      : "Unknown";

    public Brush TagBackground =>
        Item.Tags.Contains("known")    ? new SolidColorBrush(Color.FromRgb(0x2D, 0x1B, 0x0A)) :
        Item.Tags.Contains("testpin")  ? new SolidColorBrush(Color.FromRgb(0x1A, 0x20, 0x10)) :
                                         new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x24));

    public Brush TagForeground =>
        Item.Tags.Contains("known")    ? new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) :
        Item.Tags.Contains("testpin")  ? new SolidColorBrush(Color.FromRgb(0x84, 0xCC, 0x16)) :
                                         new SolidColorBrush(Color.FromRgb(0x8B, 0x8B, 0x9E));

    // Subtle left-border tint colour for the row — amber for bloatware, transparent for others
    public Brush RowAccent =>
        Item.Tags.Contains("known")   ? new SolidColorBrush(Color.FromArgb(0xCC, 0xF5, 0x9E, 0x0B)) :
        Item.Tags.Contains("testpin") ? new SolidColorBrush(Color.FromArgb(0xCC, 0x84, 0xCC, 0x16)) :
                                        Brushes.Transparent;

    public bool HasRowAccent =>
        Item.Tags.Contains("known") || Item.Tags.Contains("testpin");

    // ── Row fade-out opacity (animated in code-behind on removal) ────────────

    private double _opacity = 1.0;
    public double RowOpacity
    {
        get => _opacity;
        set { _opacity = value; OnPropertyChanged(); }
    }

    // ── Selection ────────────────────────────────────────────────────────────

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            SelectionChanged?.Invoke();
        }
    }

    public event Action? SelectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
