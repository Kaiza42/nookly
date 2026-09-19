using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nookly.Contracts.Media;
using Nookly.Contracts.Search;
using Nookly.Contracts.Discovery;
using Nookly.Contracts.Details;

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
        string? query,
        MediaType? type = null,
        int? genreId = null,
        int? year = null,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(query)) parameters.Add($"query={Uri.EscapeDataString(query)}");
        if (type is not null) parameters.Add($"type={type}");
        if (genreId is not null) parameters.Add($"genreId={genreId}");
        if (year is not null) parameters.Add($"year={year}");
        if (!string.IsNullOrWhiteSpace(actor)) parameters.Add($"actor={Uri.EscapeDataString(actor)}");
        using var response = await httpClient.GetAsync(
            $"api/media/search?{string.Join('&', parameters)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MediaSearchResultResponse[]>(
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    public async Task<MediaDetailsResponse> GetMediaDetailsAsync(
        string externalId,
        MediaType type,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/media/external/{type}/{Uri.EscapeDataString(externalId)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MediaDetailsResponse>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task<SeasonDetailsResponse> GetSeasonDetailsAsync(
        string externalId,
        int seasonNumber,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/media/external/{Uri.EscapeDataString(externalId)}/seasons/{seasonNumber}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SeasonDetailsResponse>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task<SeasonProgressResponse> GetSeasonProgressAsync(
        string externalId,
        int seasonNumber,
        int totalEpisodes,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/media/external/{Uri.EscapeDataString(externalId)}/seasons/{seasonNumber}/progress?totalEpisodes={totalEpisodes}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SeasonProgressResponse>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task UpdateSeasonProgressAsync(
        string externalId,
        int seasonNumber,
        UpdateSeasonProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/media/external/{Uri.EscapeDataString(externalId)}/seasons/{seasonNumber}/progress",
            request,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateEpisodeProgressAsync(
        string externalId,
        int seasonNumber,
        int episodeNumber,
        UpdateEpisodeProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/media/external/{Uri.EscapeDataString(externalId)}/seasons/{seasonNumber}/episodes/{episodeNumber}/progress",
            request,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
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

    public async Task SetDiscoveryPreferenceAsync(
        SetDiscoveryPreferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/media/preferences",
            request,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<DiscoveryPreferenceResponse>> GetDislikedPreferencesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            "api/media/preferences/disliked",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryPreferenceResponse[]>(
                   JsonOptions,
                   cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<DiscoveryHistoryResponse>> GetDiscoveryHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/media/history", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryHistoryResponse[]>(
                   JsonOptions,
                   cancellationToken) ?? [];
    }

    public async Task RestoreDiscoveryPreferenceAsync(
        string source,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/media/preferences/{Uri.EscapeDataString(source)}/{Uri.EscapeDataString(externalId)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
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
