using System.Windows;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;
using Nookly.Desktop.ViewModels;

namespace Nookly.Desktop;

public partial class MainWindow : Window
{
    private static readonly string[] SearchExamples =
        ["Dune", "Bleach", "Breaking Bad", "Le Seigneur des anneaux", "Arcane", "Interstellar"];

    private readonly Services.SessionStore session;
    private readonly System.Windows.Forms.NotifyIcon trayIcon;
    private readonly Services.MemberApiClient memberApiClient;
    private readonly System.Windows.Threading.DispatcherTimer activityTimer;
    private bool allowClose;
    private bool isSidebarCollapsed;
    public MainWindow(LibraryViewModel viewModel, AdminViewModel adminViewModel,
        Services.SessionStore session, Services.MemberApiClient memberApiClient)
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
        DataContext = viewModel;
        AdministrationPanel.DataContext = adminViewModel;
        AdministrationButton.Visibility = string.Equals(session.Member?.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
        CollapsedAdministrationButton.Visibility = AdministrationButton.Visibility;
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
        DiscoveryTitlePlaceholder.Text = $"Exemple : {SearchExamples[Random.Shared.Next(SearchExamples.Length)]}";
        UpdateDiscoveryTitlePlaceholder();
        await ViewModel.LoadCommand.ExecuteAsync(null);
        await SyncMemberActivityAsync();
        activityTimer.Start();
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

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        isSidebarCollapsed = !isSidebarCollapsed;
        SidebarColumn.Width = new GridLength(isSidebarCollapsed ? 64 : 220);
        SidebarBrand.Visibility = isSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        ExpandedNavigation.Visibility = isSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        CollapsedNavigation.Visibility = isSidebarCollapsed ? Visibility.Visible : Visibility.Collapsed;
        MemberButton.Visibility = isSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        CollapsedMemberButton.Visibility = isSidebarCollapsed ? Visibility.Visible : Visibility.Collapsed;
        SidebarToggleButton.Content = isSidebarCollapsed ? "\uE76C" : "\uE76B";
        SidebarToggleButton.ToolTip = isSidebarCollapsed ? "Deplier le menu" : "Replier le menu";
        SidebarToggleButton.HorizontalAlignment = isSidebarCollapsed
            ? System.Windows.HorizontalAlignment.Center
            : System.Windows.HorizontalAlignment.Right;
        SidebarToggleButton.Margin = isSidebarCollapsed ? new Thickness(0) : new Thickness(0, 0, 12, 0);
    }

    private async void DiscoveryResult_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FindVisualParent<System.Windows.Controls.Button>(e.OriginalSource as DependencyObject) is not null) return;
        if (sender is not System.Windows.Controls.ListViewItem { DataContext: MediaSearchResultViewModel item }) return;
        if (ViewModel.ShowSearchResultCommand.CanExecute(item))
            await ViewModel.ShowSearchResultCommand.ExecuteAsync(item);
    }

    private async void LibraryItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FindVisualParent<System.Windows.Controls.Button>(e.OriginalSource as DependencyObject) is not null) return;
        if (sender is not System.Windows.Controls.ListViewItem { DataContext: MediaListItemViewModel item }) return;
        if (ViewModel.ShowLibraryItemCommand.CanExecute(item))
            await ViewModel.ShowLibraryItemCommand.ExecuteAsync(item);
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T parent) return parent;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private void DiscoveryTitleInput_Changed(object sender, RoutedEventArgs e) =>
        UpdateDiscoveryTitlePlaceholder();

    private void UpdateDiscoveryTitlePlaceholder()
    {
        if (DiscoveryTitlePlaceholder is null || DiscoveryTitleInput is null) return;
        DiscoveryTitlePlaceholder.Visibility = string.IsNullOrWhiteSpace(DiscoveryTitleInput.Text) &&
                                               !DiscoveryTitleInput.IsKeyboardFocusWithin
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
