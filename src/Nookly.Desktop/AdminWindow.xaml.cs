using System.Windows;
using Nookly.Contracts.Administration;
using Nookly.Desktop.Services;

namespace Nookly.Desktop;

public partial class AdminWindow : Window
{
    private readonly AdminApiClient api; private readonly SessionStore session;
    public AdminWindow(AdminApiClient api, SessionStore session) { InitializeComponent(); this.api = api; this.session = session; Loaded += async (_, _) => await LoadAsync(); }
    private async Task LoadAsync()
    { try { MembersGrid.ItemsSource = (await api.GetMembersAsync()).Select(x => new AdminMemberItem(x)).ToArray(); } catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Administration"); } }
    private async void SendNotification_Click(object sender, RoutedEventArgs e)
    {
        if (MembersGrid.SelectedItem is not AdminMemberItem member) { System.Windows.MessageBox.Show("Selectionne un utilisateur."); return; }
        try { await api.SendNotificationAsync(new SendNotificationRequest(member.Id, NotificationTitleBox.Text, NotificationMessageBox.Text)); NotificationTitleBox.Clear(); NotificationMessageBox.Clear(); System.Windows.MessageBox.Show("Notification envoyee."); }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message, "Envoi impossible"); }
    }
    private void Logout_Click(object sender, RoutedEventArgs e) { session.ClearToken(); ((App)System.Windows.Application.Current).ShowLoginWindow(); Close(); }
    private sealed class AdminMemberItem(AdminMemberResponse member)
    {
        public Guid Id => member.Id; public string Email => member.Email; public string DisplayName => member.DisplayName;
        public bool IsEmailConfirmed => member.IsEmailConfirmed;
        public string UsageLabel
        {
            get { var duration = TimeSpan.FromSeconds(member.UsageSeconds); return duration.TotalHours >= 1 ? $"{(int)duration.TotalHours} h {duration.Minutes} min" : $"{duration.Minutes} min"; }
        }
        public string LastActivityLabel => member.LastActivityAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "Jamais";
    }
}
