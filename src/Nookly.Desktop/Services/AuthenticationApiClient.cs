using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Nookly.Contracts.Authentication;

namespace Nookly.Desktop.Services;

public sealed class AuthenticationApiClient(HttpClient client)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public Task<AuthenticationResponse> LoginAsync(LoginRequest request) => SendAsync("api/auth/login", request);
    public Task<MessageResponse> RegisterAsync(RegisterRequest request) => SendMessageAsync("api/auth/register", request);
    public Task<MessageResponse> ForgotPasswordAsync(string email) => SendMessageAsync("api/auth/forgot-password", new ForgotPasswordRequest(email));
    public Task<MessageResponse> ResetPasswordAsync(string token, string password) => SendMessageAsync("api/auth/reset-password", new ResetPasswordRequest(token, password));
    private async Task<AuthenticationResponse> SendAsync<T>(string uri, T request)
    {
        using var response = await client.PostAsJsonAsync(uri, request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            ApiError? error = null;
            try { error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions); } catch (JsonException) { }
            throw new InvalidOperationException(response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? "Email ou mot de passe incorrect."
                : error?.Error ?? "Impossible de valider ces informations.");
        }
        return await response.Content.ReadFromJsonAsync<AuthenticationResponse>(JsonOptions)
               ?? throw new InvalidOperationException("Reponse vide de l'API.");
    }
    private async Task<MessageResponse> SendMessageAsync<T>(string uri, T request)
    {
        using var response = await client.PostAsJsonAsync(uri, request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
            throw new InvalidOperationException(error?.Error ?? "Impossible de valider ces informations.");
        }
        return await response.Content.ReadFromJsonAsync<MessageResponse>(JsonOptions) ?? new MessageResponse("Operation terminee.");
    }
    private sealed record ApiError(string Error);
}
