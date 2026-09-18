using System.Net.Http;
using System.Net.Http.Json;
using Nookly.Contracts.Administration;

namespace Nookly.Desktop.Services;

public sealed class MemberApiClient(HttpClient client)
{
    public async Task RecordActivityAsync()
    { using var response = await client.PostAsync("api/member/activity", null); response.EnsureSuccessStatusCode(); }
    public async Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync() =>
        await client.GetFromJsonAsync<List<NotificationResponse>>("api/member/notifications") ?? [];
    public async Task MarkReadAsync(Guid id)
    { using var response = await client.PostAsync($"api/member/notifications/{id}/read", null); response.EnsureSuccessStatusCode(); }
}
