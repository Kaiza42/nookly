using System.Windows;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(LibraryViewModel viewModel)
    {
        InitializeComponent();
        var workArea = SystemParameters.WorkArea;
        MaxWidth = workArea.Width;
        MaxHeight = workArea.Height;
        Width = Math.Min(1120, workArea.Width);
        Height = Math.Min(720, workArea.Height);
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    public LibraryViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
