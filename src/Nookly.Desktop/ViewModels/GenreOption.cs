using CommunityToolkit.Mvvm.ComponentModel;

namespace Nookly.Desktop.ViewModels;

public partial class GenreOption(string label, int value) : ObservableObject
{
    public string Label { get; } = label;
    public int Value { get; } = value;

    [ObservableProperty]
    private bool isSelected;
}
