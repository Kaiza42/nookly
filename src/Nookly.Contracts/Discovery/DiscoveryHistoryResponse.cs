using Nookly.Contracts.Media;

namespace Nookly.Contracts.Discovery;

public sealed record DiscoveryHistoryResponse(
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
