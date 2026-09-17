using System.Windows;
using System.ComponentModel;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class MainWindow : Window
{
    private readonly Services.SessionStore session;
    private readonly System.Windows.Forms.NotifyIcon trayIcon;
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
        trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Nookly",
            ContextMenuStrip = CreateTrayMenu()
        };
        trayIcon.DoubleClick += (_, _) => RestoreFromTray();
    }

    public LibraryViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show("Voulez-vous vous deconnecter ?", "Deconnexion", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        session.ClearToken();
        ((App)System.Windows.Application.Current).ShowLoginWindow();
        allowClose = true;
        trayIcon.Dispose();
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (allowClose) return;

        var result = System.Windows.MessageBox.Show(
            "Voulez-vous garder Nookly dans la zone de notification ?\n\n" +
            "Oui : masquer pres de l'horloge\nNon : fermer completement",
            "Fermer Nookly",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            trayIcon.Dispose();
            return;
        }

        e.Cancel = true;
        Hide();
        ShowInTaskbar = false;
        trayIcon.Visible = true;
        trayIcon.ShowBalloonTip(1500, "Nookly", "Nookly continue de fonctionner ici.",
            System.Windows.Forms.ToolTipIcon.Info);
    }

    private System.Windows.Forms.ContextMenuStrip CreateTrayMenu()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Ouvrir Nookly", null, (_, _) => RestoreFromTray());
        menu.Items.Add("Quitter", null, (_, _) => ExitFromTray());
        return menu;
    }

    private void RestoreFromTray()
    {
        Dispatcher.Invoke(() =>
        {
            trayIcon.Visible = false;
            ShowInTaskbar = true;
            Show();
            WindowState = WindowState.Normal;
            Activate();
        });
    }

    private void ExitFromTray()
    {
        Dispatcher.Invoke(() =>
        {
            allowClose = true;
            trayIcon.Dispose();
            Close();
        });
    }

    private void StaySignedIn_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) session.SetStaySignedIn(StaySignedInCheckBox.IsChecked == true);
    }
}
