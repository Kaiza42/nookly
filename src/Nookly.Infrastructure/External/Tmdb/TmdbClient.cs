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
        string? query,
        MediaType? type = null,
        int? genreId = null,
        int? year = null,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var token = options.Value.ReadAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "TMDB is not configured. Set Tmdb:ReadAccessToken in the API configuration.");
        }

        if (!string.IsNullOrWhiteSpace(query) && type is null && genreId is null && year is null &&
            string.IsNullOrWhiteSpace(actor))
        {
            var payload = await GetAsync<TmdbSearchResponse>(
                $"search/multi?query={Uri.EscapeDataString(query.Trim())}&include_adult=false&language=fr-FR&page=1",
                token,
                cancellationToken);
            var searchResults = payload?.Results
                .Where(result => result.MediaType is "movie" or "tv")
                .Select(item => MapResult(item, item.MediaType!))
                .Where(result => !string.IsNullOrWhiteSpace(result.Title))
                .Take(12)
                .ToArray() ?? [];
            return await AddCastAsync(searchResults, token, cancellationToken);
        }

        var actorId = await ResolveActorIdAsync(actor, token, cancellationToken);
        if (!string.IsNullOrWhiteSpace(actor) && actorId is null)
        {
            return [];
        }

        var mediaKinds = type switch
        {
            MediaType.Movie => new[] { "movie" },
            MediaType.TvSeries or MediaType.Anime => new[] { "tv" },
            _ => new[] { "movie", "tv" }
        };
        var isRandomDiscovery = string.IsNullOrWhiteSpace(query) && type is null &&
                                genreId is null && year is null && actorId is null;
        var page = isRandomDiscovery ? Random.Shared.Next(1, 11) : 1;

        var results = new List<MediaSearchResult>();
        foreach (var mediaKind in mediaKinds)
        {
            var parameters = new List<string>
            {
                "include_adult=false", "language=fr-FR", $"page={page}", "sort_by=popularity.desc"
            };
            var genres = new List<int>();
            if (genreId is not null) genres.Add(MapGenreId(genreId.Value, mediaKind));
            if (year is not null) parameters.Add(mediaKind == "movie" ? $"primary_release_year={year}" : $"first_air_date_year={year}");
            if (actorId is not null) parameters.Add($"with_cast={actorId}");
            if (type == MediaType.Anime)
            {
                if (!genres.Contains(16)) genres.Add(16);
                parameters.Add("with_original_language=ja");
            }
            if (genres.Count > 0) parameters.Add($"with_genres={string.Join(',', genres)}");

            var payload = await GetAsync<TmdbSearchResponse>(
                $"discover/{mediaKind}?{string.Join('&', parameters)}",
                token,
                cancellationToken);
            results.AddRange(payload?.Results.Select(item => MapResult(item, mediaKind)) ?? []);
        }

        var titleQuery = query?.Trim();
        var filteredResults = results
            .Where(result => string.IsNullOrWhiteSpace(titleQuery) ||
                             result.Title.Contains(titleQuery, StringComparison.CurrentCultureIgnoreCase));
        if (isRandomDiscovery)
        {
            filteredResults = filteredResults.OrderBy(_ => Random.Shared.Next());
        }

        var selectedResults = filteredResults
            .Take(12)
            .ToArray();
        return await AddCastAsync(selectedResults, token, cancellationToken);
    }

    private async Task<IReadOnlyList<MediaSearchResult>> AddCastAsync(
        IReadOnlyList<MediaSearchResult> results,
        string token,
        CancellationToken cancellationToken)
    {
        return await Task.WhenAll(results.Select(async result =>
        {
            try
            {
                var mediaKind = result.Type == MediaType.Movie ? "movie" : "tv";
                var credits = await GetAsync<TmdbCreditsResponse>(
                    $"{mediaKind}/{result.ExternalId}/credits?language=fr-FR",
                    token,
                    cancellationToken);
                return result with
                {
                    Cast = credits?.Cast
                        .Where(person => !string.IsNullOrWhiteSpace(person.Name))
                        .Take(3)
                        .Select(person => person.Name)
                        .ToArray() ?? []
                };
            }
            catch (HttpRequestException)
            {
                return result;
            }
        }));
    }

    private async Task<long?> ResolveActorIdAsync(
        string? actor,
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor)) return null;
        var payload = await GetAsync<TmdbPersonSearchResponse>(
            $"search/person?query={Uri.EscapeDataString(actor.Trim())}&include_adult=false&language=fr-FR&page=1",
            token,
            cancellationToken);
        return payload?.Results.FirstOrDefault()?.Id;
    }

    private static int MapGenreId(int genreId, string mediaKind)
    {
        if (mediaKind == "movie") return genreId;
        return genreId switch
        {
            28 or 12 => 10759,
            14 or 878 => 10765,
            _ => genreId
        };
    }

    private async Task<T?> GetAsync<T>(string uri, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private static MediaSearchResult MapResult(TmdbSearchItem item, string mediaType)
    {
        var releaseDateText = mediaType == "movie" ? item.ReleaseDate : item.FirstAirDate;
        DateOnly? releaseDate = DateOnly.TryParse(releaseDateText, out var parsedDate)
            ? parsedDate
            : null;

        return new MediaSearchResult(
            "tmdb",
            item.Id.ToString(),
            item.Title ?? item.Name ?? string.Empty,
            item.Overview,
            GetMediaType(item, mediaType),
            item.PosterPath is null ? null : $"{PosterBaseUrl}{item.PosterPath}",
            item.VoteAverage,
            releaseDate);
    }

    private static MediaType GetMediaType(TmdbSearchItem item, string mediaType)
    {
        var isJapaneseAnimation = (item.GenreIds?.Contains(16) ?? false) &&
                                  (item.OriginalLanguage == "ja" ||
                                   (item.OriginCountry?.Contains("JP") ?? false));
        if (isJapaneseAnimation)
        {
            return MediaType.Anime;
        }

        return mediaType == "movie" ? MediaType.Movie : MediaType.TvSeries;
    }

    private sealed record TmdbSearchResponse(
        [property: JsonPropertyName("results")] TmdbSearchItem[] Results);

    private sealed record TmdbPersonSearchResponse(
        [property: JsonPropertyName("results")] TmdbPerson[] Results);

    private sealed record TmdbPerson([property: JsonPropertyName("id")] long Id);

    private sealed record TmdbCreditsResponse(
        [property: JsonPropertyName("cast")] TmdbCastMember[] Cast);

    private sealed record TmdbCastMember(
        [property: JsonPropertyName("name")] string Name);

    private sealed record TmdbSearchItem(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("media_type")] string? MediaType,
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
