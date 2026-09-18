using System.Collections.ObjectModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nookly.Contracts.Administration;
using Nookly.Desktop.Services;

namespace Nookly.Desktop.ViewModels;

public partial class AdminViewModel(AdminApiClient api) : ObservableObject
{
    public ObservableCollection<AdminMemberViewModel> Members { get; } = [];

    [ObservableProperty] private AdminMemberViewModel? selectedMember;
    [ObservableProperty] private string notificationTitle = string.Empty;
    [ObservableProperty] private string notificationMessage = string.Empty;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isLoading;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            Members.Clear();
            foreach (var member in await api.GetMembersAsync()) Members.Add(new AdminMemberViewModel(member));
            StatusMessage = null;
        }
        catch (HttpRequestException) { StatusMessage = "Impossible de charger les utilisateurs."; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SendNotificationAsync()
    {
        if (SelectedMember is null || string.IsNullOrWhiteSpace(NotificationTitle) || string.IsNullOrWhiteSpace(NotificationMessage))
        { StatusMessage = "Selectionne un utilisateur et renseigne le titre et le message."; return; }
        try
        {
            await api.SendNotificationAsync(new SendNotificationRequest(SelectedMember.Id, NotificationTitle.Trim(), NotificationMessage.Trim()));
            NotificationTitle = NotificationMessage = string.Empty;
            StatusMessage = "Notification envoyee.";
        }
        catch (HttpRequestException) { StatusMessage = "Impossible d'envoyer la notification."; }
    }
}
