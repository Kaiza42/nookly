using Nookly.Contracts.Media;

namespace Nookly.Contracts.Details;

public sealed record MediaDetailsResponse(
    string ExternalId,
    string Title,
    MediaType Type,
    string? Description,
    string? PosterUrl,
    string? BackdropUrl,
    DateOnly? ReleaseDate,
    string? AirStatus,
    int? RuntimeMinutes,
    decimal? CommunityRating,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Directors,
    IReadOnlyList<string> Cast,
    string? TrailerUrl,
    IReadOnlyList<SeasonSummaryResponse> Seasons);

public sealed record SeasonSummaryResponse(
    int Number,
    string Title,
    string? Description,
    string? PosterUrl,
    DateOnly? AirDate,
    int EpisodeCount,
    decimal? CommunityRating);

public sealed record SeasonDetailsResponse(
    int Number,
    string Title,
    string? Description,
    string? PosterUrl,
    DateOnly? AirDate,
    decimal? CommunityRating,
    IReadOnlyList<EpisodeResponse> Episodes);

public sealed record EpisodeResponse(
    int Number,
    string Title,
    string? Description,
    string? ImageUrl,
    DateOnly? AirDate,
    int? RuntimeMinutes,
    decimal? CommunityRating);
