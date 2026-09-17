using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Nookly.Contracts.Authentication;

namespace Nookly.Desktop.Services;

public sealed class AuthenticationApiClient(HttpClient client)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public Task<AuthenticationResponse> LoginAsync(LoginRequest request) => SendAsync("api/auth/login", request);
    public Task<AuthenticationResponse> RegisterAsync(RegisterRequest request) => SendAsync("api/auth/register", request);
    private async Task<AuthenticationResponse> SendAsync<T>(string uri, T request)
    {
        using var response = await client.PostAsJsonAsync(uri, request, JsonOptions);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException(response.StatusCode == System.Net.HttpStatusCode.Unauthorized
            ? "Email ou mot de passe incorrect." : "Impossible de valider ces informations.");
        return await response.Content.ReadFromJsonAsync<AuthenticationResponse>(JsonOptions)
               ?? throw new InvalidOperationException("Reponse vide de l'API.");
    }
}
