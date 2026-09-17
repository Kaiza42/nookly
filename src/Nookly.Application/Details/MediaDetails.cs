using Nookly.Domain.Media;

namespace Nookly.Application.Details;

public sealed record MediaDetails(
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
    IReadOnlyList<SeasonSummary> Seasons);

public sealed record SeasonSummary(
    int Number,
    string Title,
    string? Description,
    string? PosterUrl,
    DateOnly? AirDate,
    int EpisodeCount,
    decimal? CommunityRating);

public sealed record SeasonDetails(
    int Number,
    string Title,
    string? Description,
    string? PosterUrl,
    DateOnly? AirDate,
    decimal? CommunityRating,
    IReadOnlyList<EpisodeDetails> Episodes);

public sealed record EpisodeDetails(
    int Number,
    string Title,
    string? Description,
    string? ImageUrl,
    DateOnly? AirDate,
    int? RuntimeMinutes,
    decimal? CommunityRating);
