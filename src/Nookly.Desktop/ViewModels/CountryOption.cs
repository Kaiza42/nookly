using CommunityToolkit.Mvvm.ComponentModel;

namespace Nookly.Desktop.ViewModels;

public partial class CountryOption(string label, string code) : ObservableObject
{
    public string Label { get; } = label;
    public string Code { get; } = code;

    [ObservableProperty]
    private bool isSelected;
}
