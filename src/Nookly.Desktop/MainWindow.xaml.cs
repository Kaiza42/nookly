using System.Windows;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(LibraryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    public LibraryViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
