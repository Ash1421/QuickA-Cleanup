using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using QuickA_Cleanup.Core.Models;

namespace QuickA_Cleanup.GUI;

public class ItemViewModel : INotifyPropertyChanged
{
    public QuickAccessItem Item { get; }

    public ItemViewModel(QuickAccessItem item) => Item = item;

    public string Name => Item.Name;
    public string Guid => Item.Guid;

    public string TagLabel =>
        Item.Tags.Contains("known")   ? "Bloatware" :
        Item.Tags.Contains("testpin") ? "Test"      : "Unknown";

    public Brush TagBackground =>
        Item.Tags.Contains("known")   ? new SolidColorBrush(Color.FromArgb(38, 0xF5, 0x9E, 0x0B)) :
        Item.Tags.Contains("testpin") ? new SolidColorBrush(Color.FromArgb(38, 0x84, 0xCC, 0x16)) :
                                         new SolidColorBrush(Color.FromArgb(30, 0x80, 0x80, 0x80));

    public Brush TagForeground =>
        Item.Tags.Contains("known")   ? new SolidColorBrush(Color.FromArgb(255, 0xF5, 0x9E, 0x0B)) :
        Item.Tags.Contains("testpin") ? new SolidColorBrush(Color.FromArgb(255, 0x84, 0xCC, 0x16)) :
                                         new SolidColorBrush(Color.FromArgb(255, 0x71, 0x71, 0x7A));

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
