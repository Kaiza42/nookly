using System.Windows;
using System.ComponentModel;
using System.Net.Http;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class MainWindow : Window
{
    private readonly Services.SessionStore session;
    private readonly System.Windows.Forms.NotifyIcon trayIcon;
    private readonly Services.MemberApiClient memberApiClient;
    private readonly Services.AppUpdateService updateService;
    private readonly System.Windows.Threading.DispatcherTimer activityTimer;
    private bool allowClose;
    public MainWindow(LibraryViewModel viewModel, AdminViewModel adminViewModel,
        Services.SessionStore session, Services.MemberApiClient memberApiClient,
        Services.AppUpdateService updateService)
    {
        InitializeComponent();
        var workArea = SystemParameters.WorkArea;
        MaxWidth = workArea.Width;
        MaxHeight = workArea.Height;
        Width = Math.Min(1120, workArea.Width);
        Height = Math.Min(720, workArea.Height);
        ViewModel = viewModel;
        this.session = session;
        this.memberApiClient = memberApiClient;
        this.updateService = updateService;
        DataContext = viewModel;
        AdministrationPanel.DataContext = adminViewModel;
        AdministrationButton.Visibility = string.Equals(session.Member?.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
        MemberButton.Content = $"{session.Member?.DisplayName ?? "Membre"}  |  Deconnexion";
        StaySignedInCheckBox.IsChecked = session.StaySignedIn;
        trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "Nookly",
            ContextMenuStrip = CreateTrayMenu()
        };
        trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        activityTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        activityTimer.Tick += async (_, _) => await SyncMemberActivityAsync();
    }

    public LibraryViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
        await SyncMemberActivityAsync();
        activityTimer.Start();
        CurrentVersionText.Text = $"Version {updateService.CurrentVersion}";
        await CheckForUpdatesAsync(false);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show("Voulez-vous vous deconnecter ?", "Deconnexion", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        session.ClearToken();
        activityTimer.Stop();
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
            activityTimer.Stop();
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
            activityTimer.Stop();
            trayIcon.Dispose();
            Close();
        });
    }

    private async Task SyncMemberActivityAsync()
    {
        try
        {
            await memberApiClient.RecordActivityAsync();
            foreach (var notification in await memberApiClient.GetNotificationsAsync())
            {
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(5000, notification.Title, notification.Message, System.Windows.Forms.ToolTipIcon.Info);
                await memberApiClient.MarkReadAsync(notification.Id);
            }
        }
        catch (HttpRequestException) { }
    }

    private void StaySignedIn_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) session.SetStaySignedIn(StaySignedInCheckBox.IsChecked == true);
    }

    private async void Administration_Click(object sender, RoutedEventArgs e)
    {
        if (AdministrationPanel.DataContext is AdminViewModel adminViewModel)
            await adminViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e) => await CheckForUpdatesAsync(true);

    private async Task CheckForUpdatesAsync(bool showNoUpdateMessage)
    {
        if (!updateService.IsInstalled)
        {
            if (showNoUpdateMessage) UpdateStatusText.Text = "Les mises a jour sont disponibles dans la version installee de Nookly.";
            return;
        }

        try
        {
            UpdateStatusText.Text = "Recherche d'une mise a jour...";
            var update = await updateService.CheckAsync();
            if (update is null)
            {
                UpdateStatusText.Text = showNoUpdateMessage ? "Nookly est a jour." : string.Empty;
                return;
            }

            UpdateStatusText.Text = $"Version {update.TargetFullRelease.Version} disponible.";
            if (System.Windows.MessageBox.Show(
                    $"La version {update.TargetFullRelease.Version} est disponible.\n\nLa telecharger et redemarrer Nookly ?",
                    "Mise a jour Nookly", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes) return;

            await updateService.DownloadAsync(update, progress => Dispatcher.Invoke(() =>
                UpdateStatusText.Text = $"Telechargement : {progress} %"));
            UpdateStatusText.Text = "Installation et redemarrage...";
            allowClose = true;
            activityTimer.Stop();
            trayIcon.Dispose();
            updateService.ApplyAndRestart(update);
        }
        catch (Exception)
        {
            UpdateStatusText.Text = showNoUpdateMessage
                ? "Impossible de verifier les mises a jour pour le moment."
                : string.Empty;
        }
    }
}
