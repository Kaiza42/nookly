using Nookly.Contracts.Administration;

namespace Nookly.Desktop.ViewModels;

public sealed class AdminMemberViewModel(AdminMemberResponse member)
{
    public Guid Id => member.Id;
    public string Email => member.Email;
    public string DisplayName => member.DisplayName;
    public bool IsEmailConfirmed => member.IsEmailConfirmed;
    public string UsageLabel
    {
        get
        {
            var duration = TimeSpan.FromSeconds(member.UsageSeconds);
            return duration.TotalHours >= 1
                ? $"{(int)duration.TotalHours} h {duration.Minutes} min"
                : $"{duration.Minutes} min";
        }
    }
    public string LastActivityLabel => member.LastActivityAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "Jamais";
}
