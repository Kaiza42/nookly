using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nookly.Contracts.Media;
using Nookly.Contracts.Search;

namespace Nookly.Desktop.Services;

public sealed class MediaApiClient(HttpClient httpClient) : IMediaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public async Task<IReadOnlyList<MediaItemResponse>> GetMediaAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/media", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MediaItemResponse[]>(
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    public async Task<IReadOnlyList<MediaSearchResultResponse>> SearchMediaAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/media/search?query={Uri.EscapeDataString(query)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MediaSearchResultResponse[]>(
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    public async Task<MediaItemResponse> CreateMediaAsync(
        CreateMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/media",
            request,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MediaItemResponse>(
                   JsonOptions,
                   cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task<MediaItemResponse> UpdateMediaAsync(
        Guid id,
        UpdateMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/media/{id}",
            request,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MediaItemResponse>(
                   JsonOptions,
                   cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task DeleteMediaAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"api/media/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
