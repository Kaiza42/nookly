using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Nookly.Application.Abstractions;
using Nookly.Application.Search;
using Nookly.Domain.Media;

namespace Nookly.Infrastructure.External.Tmdb;

public sealed class TmdbClient(HttpClient httpClient, IOptions<TmdbOptions> options)
    : IExternalMediaSearch
{
    private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w500";

    public async Task<IReadOnlyList<MediaSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var token = options.Value.ReadAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "TMDB is not configured. Set Tmdb:ReadAccessToken in the API configuration.");
        }

        var uri = $"search/multi?query={Uri.EscapeDataString(query.Trim())}" +
                  "&include_adult=false&language=fr-FR&page=1";
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>(
            cancellationToken: cancellationToken);

        return payload?.Results
            .Where(result => result.MediaType is "movie" or "tv")
            .Select(MapResult)
            .Where(result => !string.IsNullOrWhiteSpace(result.Title))
            .Take(20)
            .ToArray() ?? [];
    }

    private static MediaSearchResult MapResult(TmdbSearchItem item)
    {
        var releaseDateText = item.MediaType == "movie" ? item.ReleaseDate : item.FirstAirDate;
        DateOnly? releaseDate = DateOnly.TryParse(releaseDateText, out var parsedDate)
            ? parsedDate
            : null;

        return new MediaSearchResult(
            "tmdb",
            item.Id.ToString(),
            item.Title ?? item.Name ?? string.Empty,
            item.Overview,
            GetMediaType(item),
            item.PosterPath is null ? null : $"{PosterBaseUrl}{item.PosterPath}",
            item.VoteAverage,
            releaseDate);
    }

    private static MediaType GetMediaType(TmdbSearchItem item)
    {
        var isJapaneseAnimation = (item.GenreIds?.Contains(16) ?? false) &&
                                  (item.OriginalLanguage == "ja" ||
                                   (item.OriginCountry?.Contains("JP") ?? false));
        if (isJapaneseAnimation)
        {
            return MediaType.Anime;
        }

        return item.MediaType == "movie" ? MediaType.Movie : MediaType.TvSeries;
    }

    private sealed record TmdbSearchResponse(
        [property: JsonPropertyName("results")] TmdbSearchItem[] Results);

    private sealed record TmdbSearchItem(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("media_type")] string MediaType,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("genre_ids")] int[]? GenreIds,
        [property: JsonPropertyName("original_language")] string? OriginalLanguage,
        [property: JsonPropertyName("origin_country")] string[]? OriginCountry,
        [property: JsonPropertyName("vote_average")] decimal? VoteAverage,
        [property: JsonPropertyName("release_date")] string? ReleaseDate,
        [property: JsonPropertyName("first_air_date")] string? FirstAirDate);
}
