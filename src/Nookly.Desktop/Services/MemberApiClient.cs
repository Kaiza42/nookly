using System.Net.Http;
using System.Net.Http.Json;
using Nookly.Contracts.Administration;
using Nookly.Contracts.Authentication;

namespace Nookly.Desktop.Services;

public sealed class MemberApiClient(HttpClient client)
{
    public async Task<MemberResponse> GetProfileAsync() =>
        await client.GetFromJsonAsync<MemberResponse>("api/auth/me")
        ?? throw new HttpRequestException("Le profil membre est indisponible.");

    public async Task RecordActivityAsync()
    { using var response = await client.PostAsync("api/member/activity", null); response.EnsureSuccessStatusCode(); }
    public async Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync() =>
        await client.GetFromJsonAsync<List<NotificationResponse>>("api/member/notifications") ?? [];
    public async Task MarkReadAsync(Guid id)
    { using var response = await client.PostAsync($"api/member/notifications/{id}/read", null); response.EnsureSuccessStatusCode(); }
}
