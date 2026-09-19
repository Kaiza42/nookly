using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Nookly.Application.Abstractions;
using Nookly.Application.Details;
using Nookly.Application.Search;
using Nookly.Domain.Media;

namespace Nookly.Infrastructure.External.Tmdb;

public sealed class TmdbClient(HttpClient httpClient, IOptions<TmdbOptions> options)
    : IExternalMediaSearch
{
    private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w500";

    public async Task<string?> GetTitleAsync(
        string externalId,
        MediaType type,
        CancellationToken cancellationToken = default)
    {
        if (type == MediaType.Manga) return null;
        var mediaKind = type == MediaType.Movie ? "movie" : "tv";
        var details = await GetAsync<TmdbDetailsResponse>(
            $"{mediaKind}/{externalId}?language=fr-FR",
            GetToken(),
            cancellationToken);
        return details?.Title ?? details?.Name;
    }

    public async Task<MediaDetails?> GetDetailsAsync(
        string externalId,
        MediaType type,
        CancellationToken cancellationToken = default)
    {
        if (type == MediaType.Manga) return null;
        var mediaKind = type == MediaType.Movie ? "movie" : "tv";
        var details = await GetAsync<TmdbFullDetailsResponse>(
            $"{mediaKind}/{externalId}?language=fr-FR&append_to_response=credits,videos",
            GetToken(),
            cancellationToken);
        if (details is null) return null;

        var directors = details.Credits?.Crew
            .Where(person => person.Job == "Director")
            .Select(person => person.Name)
            .Concat(details.CreatedBy?.Select(person => person.Name) ?? [])
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Take(4)
            .ToArray() ?? [];
        var trailer = details.Videos?.Results.FirstOrDefault(video =>
            video.Site == "YouTube" && video.Type == "Trailer" && video.Official)
            ?? details.Videos?.Results.FirstOrDefault(video =>
                video.Site == "YouTube" && video.Type == "Trailer");

        return new MediaDetails(
            externalId,
            details.Title ?? details.Name ?? string.Empty,
            type,
            details.Overview,
            ToImageUrl(details.PosterPath),
            ToImageUrl(details.BackdropPath),
            ParseDate(details.ReleaseDate ?? details.FirstAirDate),
            TranslateStatus(details.Status),
            details.Runtime ?? details.EpisodeRunTime?.FirstOrDefault(),
            details.VoteAverage,
            details.Genres?.Select(genre => genre.Name).ToArray() ?? [],
            directors,
            details.Credits?.Cast.Select(person => person.Name).Take(8).ToArray() ?? [],
            trailer is null ? null : $"https://www.youtube.com/watch?v={trailer.Key}",
            details.Seasons?
                .Where(season => season.SeasonNumber > 0)
                .Select(season => new SeasonSummary(
                    season.SeasonNumber,
                    season.Name,
                    season.Overview,
                    ToImageUrl(season.PosterPath),
                    ParseDate(season.AirDate),
                    season.EpisodeCount,
                    season.VoteAverage))
                .OrderBy(season => season.Number)
                .ToArray() ?? []);
    }

    public async Task<SeasonDetails?> GetSeasonAsync(
        string externalId,
        int seasonNumber,
        CancellationToken cancellationToken = default)
    {
        var season = await GetAsync<TmdbSeasonResponse>(
            $"tv/{externalId}/season/{seasonNumber}?language=fr-FR",
            GetToken(),
            cancellationToken);
        return season is null
            ? null
            : new SeasonDetails(
                season.SeasonNumber,
                season.Name,
                season.Overview,
                ToImageUrl(season.PosterPath),
                ParseDate(season.AirDate),
                season.VoteAverage,
                season.Episodes.Select(episode => new EpisodeDetails(
                    episode.EpisodeNumber,
                    episode.Name,
                    episode.Overview,
                    ToImageUrl(episode.StillPath),
                    ParseDate(episode.AirDate),
                    episode.Runtime,
                    episode.VoteAverage)).ToArray());
    }

    public async Task<IReadOnlyList<MediaSearchResult>> RecommendAsync(
        IReadOnlyList<RecommendationSeed> seeds,
        CancellationToken cancellationToken = default)
    {
        var token = GetToken();
        var requests = seeds
            .Where(seed => seed.Type != MediaType.Manga)
            .Select(async seed =>
            {
                var mediaKind = seed.Type == MediaType.Movie ? "movie" : "tv";
                var payload = await GetAsync<TmdbSearchResponse>(
                    $"{mediaKind}/{seed.ExternalId}/recommendations?language=fr-FR&page=1",
                    token,
                    cancellationToken);
                return payload?.Results.Select(item => MapResult(item, mediaKind)) ?? [];
            });

        var recommendations = (await Task.WhenAll(requests))
            .SelectMany(items => items)
            .GroupBy(item => item.ExternalId)
            .Select(group => group.First())
            .OrderBy(_ => Random.Shared.Next())
            .Take(12)
            .ToArray();
        return await AddCastAsync(recommendations, token, cancellationToken);
    }

    public async Task<IReadOnlyList<MediaSearchResult>> SearchAsync(
        string? query,
        IReadOnlyCollection<MediaType>? types = null,
        IReadOnlyCollection<int>? genreIds = null,
        int? year = null,
        string? actor = null,
        string? director = null,
        IReadOnlyCollection<string>? countries = null,
        CancellationToken cancellationToken = default)
    {
        var token = GetToken();

        if (!string.IsNullOrWhiteSpace(query) && types is not { Count: > 0 } &&
            genreIds is not { Count: > 0 } && year is null &&
            string.IsNullOrWhiteSpace(actor) && string.IsNullOrWhiteSpace(director) &&
            countries is not { Count: > 0 })
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

        var actorId = await ResolvePersonIdAsync(actor, token, cancellationToken);
        if (!string.IsNullOrWhiteSpace(actor) && actorId is null)
        {
            return [];
        }
        var directorId = await ResolvePersonIdAsync(director, token, cancellationToken);
        if (!string.IsNullOrWhiteSpace(director) && directorId is null)
        {
            return [];
        }

        var selectedTypes = types is { Count: > 0 }
            ? types.Distinct().ToArray()
            : [MediaType.Movie, MediaType.TvSeries];
        var selectedGenres = genreIds?.Distinct().ToArray() ?? [];
        var selectedCountries = countries?
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];
        var isRandomDiscovery = string.IsNullOrWhiteSpace(query) && types is not { Count: > 0 } &&
                                selectedGenres.Length == 0 && year is null && actorId is null && directorId is null &&
                                selectedCountries.Length == 0;
        var page = isRandomDiscovery ? Random.Shared.Next(1, 11) : 1;

        var results = new List<MediaSearchResult>();
        foreach (var selectedType in selectedTypes)
        {
            var mediaKind = selectedType == MediaType.Movie ? "movie" : "tv";
            var parameters = new List<string>
            {
                "include_adult=false", "language=fr-FR", $"page={page}", "sort_by=popularity.desc"
            };
            var genres = selectedGenres.Select(genreId => MapGenreId(genreId, mediaKind)).Distinct().ToList();
            if (year is not null) parameters.Add(mediaKind == "movie" ? $"primary_release_year={year}" : $"first_air_date_year={year}");
            if (actorId is not null) parameters.Add($"with_cast={actorId}");
            if (directorId is not null) parameters.Add($"with_crew={directorId}");
            if (selectedCountries.Length > 0) parameters.Add($"with_origin_country={string.Join('|', selectedCountries)}");
            if (selectedType == MediaType.Anime)
            {
                if (!genres.Contains(16)) genres.Add(16);
                parameters.Add("with_original_language=ja");
            }
            if (genres.Count > 0) parameters.Add($"with_genres={string.Join('|', genres)}");

            var payload = await GetAsync<TmdbSearchResponse>(
                $"discover/{mediaKind}?{string.Join('&', parameters)}",
                token,
                cancellationToken);
            results.AddRange(payload?.Results.Select(item =>
                MapResult(item, mediaKind) with { Type = selectedType }) ?? []);
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
            .DistinctBy(result => new { result.ExternalId, result.Type })
            .Take(12)
            .ToArray();
        return await AddCastAsync(selectedResults, token, cancellationToken);
    }

    private string GetToken()
    {
        var token = options.Value.ReadAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "TMDB is not configured. Set Tmdb:ReadAccessToken in the API configuration.");
        }

        return token;
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

    private async Task<long?> ResolvePersonIdAsync(
        string? person,
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(person)) return null;
        var payload = await GetAsync<TmdbPersonSearchResponse>(
            $"search/person?query={Uri.EscapeDataString(person.Trim())}&include_adult=false&language=fr-FR&page=1",
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

    private static string? ToImageUrl(string? path) =>
        string.IsNullOrWhiteSpace(path) ? null : $"{PosterBaseUrl}{path}";

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, out var date) ? date : null;

    private static string? TranslateStatus(string? status) => status switch
    {
        "Returning Series" => "En diffusion",
        "Ended" => "Terminee",
        "Canceled" => "Annulee",
        "In Production" => "En production",
        "Released" => "Sorti",
        "Post Production" => "Post-production",
        "Planned" => "Planifie",
        _ => status
    };

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

    private sealed record TmdbDetailsResponse(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("name")] string? Name);

    private sealed record TmdbFullDetailsResponse(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("backdrop_path")] string? BackdropPath,
        [property: JsonPropertyName("release_date")] string? ReleaseDate,
        [property: JsonPropertyName("first_air_date")] string? FirstAirDate,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("runtime")] int? Runtime,
        [property: JsonPropertyName("episode_run_time")] int[]? EpisodeRunTime,
        [property: JsonPropertyName("vote_average")] decimal? VoteAverage,
        [property: JsonPropertyName("genres")] TmdbGenre[]? Genres,
        [property: JsonPropertyName("created_by")] TmdbPersonWithName[]? CreatedBy,
        [property: JsonPropertyName("credits")] TmdbFullCredits? Credits,
        [property: JsonPropertyName("videos")] TmdbVideos? Videos,
        [property: JsonPropertyName("seasons")] TmdbSeasonSummary[]? Seasons);

    private sealed record TmdbGenre([property: JsonPropertyName("name")] string Name);
    private sealed record TmdbPersonWithName([property: JsonPropertyName("name")] string Name);
    private sealed record TmdbFullCredits(
        [property: JsonPropertyName("cast")] TmdbCastMember[] Cast,
        [property: JsonPropertyName("crew")] TmdbCrewMember[] Crew);
    private sealed record TmdbCrewMember(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("job")] string Job);
    private sealed record TmdbVideos([property: JsonPropertyName("results")] TmdbVideo[] Results);
    private sealed record TmdbVideo(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("site")] string Site,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("official")] bool Official);
    private sealed record TmdbSeasonSummary(
        [property: JsonPropertyName("season_number")] int SeasonNumber,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("air_date")] string? AirDate,
        [property: JsonPropertyName("episode_count")] int EpisodeCount,
        [property: JsonPropertyName("vote_average")] decimal? VoteAverage);
    private sealed record TmdbSeasonResponse(
        [property: JsonPropertyName("season_number")] int SeasonNumber,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("air_date")] string? AirDate,
        [property: JsonPropertyName("vote_average")] decimal? VoteAverage,
        [property: JsonPropertyName("episodes")] TmdbEpisode[] Episodes);
    private sealed record TmdbEpisode(
        [property: JsonPropertyName("episode_number")] int EpisodeNumber,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("still_path")] string? StillPath,
        [property: JsonPropertyName("air_date")] string? AirDate,
        [property: JsonPropertyName("runtime")] int? Runtime,
        [property: JsonPropertyName("vote_average")] decimal? VoteAverage);

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
