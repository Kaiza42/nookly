using System.Windows;
using System.ComponentModel;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class MainWindow : Window
{
    private readonly Services.SessionStore session;
    private bool allowClose;
    public MainWindow(LibraryViewModel viewModel, Services.SessionStore session)
    {
        InitializeComponent();
        var workArea = SystemParameters.WorkArea;
        MaxWidth = workArea.Width;
        MaxHeight = workArea.Height;
        Width = Math.Min(1120, workArea.Width);
        Height = Math.Min(720, workArea.Height);
        ViewModel = viewModel;
        this.session = session;
        DataContext = viewModel;
        MemberButton.Content = $"{session.Member?.DisplayName ?? "Membre"}  |  Deconnexion";
        StaySignedInCheckBox.IsChecked = session.StaySignedIn;
    }

    public LibraryViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Voulez-vous vous deconnecter ?", "Deconnexion", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        session.ClearToken();
        ((App)Application.Current).ShowLoginWindow();
        allowClose = true;
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (allowClose) return;

        var result = MessageBox.Show(
            "Voulez-vous garder Nookly dans la barre des taches ?\n\n" +
            "Oui : reduire dans la barre des taches\nNon : fermer completement",
            "Fermer Nookly",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        e.Cancel = true;
        WindowState = WindowState.Minimized;
    }

    private void StaySignedIn_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) session.SetStaySignedIn(StaySignedInCheckBox.IsChecked == true);
    }
}
