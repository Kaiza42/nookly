using Nookly.Domain.Media;

namespace Nookly.Application.Discovery;

public sealed record DiscoveryHistoryDto(
    string ExternalSource,
    string ExternalId,
    MediaType MediaType,
    string Title,
    string? Description,
    string? PosterUrl,
    decimal? CommunityRating,
    DateOnly? ReleaseDate,
    string? CastLabel,
    DateTimeOffset ViewedAtUtc);
