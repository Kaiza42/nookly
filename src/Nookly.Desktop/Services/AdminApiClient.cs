using System.Net.Http;
using System.Net.Http.Json;
using Nookly.Contracts.Administration;

namespace Nookly.Desktop.Services;

public sealed class AdminApiClient(HttpClient client)
{
    public async Task<IReadOnlyList<AdminMemberResponse>> GetMembersAsync() =>
        await client.GetFromJsonAsync<List<AdminMemberResponse>>("api/admin/members") ?? [];
    public async Task SendNotificationAsync(SendNotificationRequest request)
    {
        using var response = await client.PostAsJsonAsync("api/admin/notifications", request);
        response.EnsureSuccessStatusCode();
    }
}
