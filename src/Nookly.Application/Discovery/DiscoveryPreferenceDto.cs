using Nookly.Domain.Media;

namespace Nookly.Application.Discovery;

public sealed record DiscoveryPreferenceDto(
    string ExternalSource,
    string ExternalId,
    string Title,
    MediaType? MediaType,
    bool IsLiked,
    string? Description = null,
    string? PosterUrl = null,
    decimal? CommunityRating = null,
    DateOnly? ReleaseDate = null,
    IReadOnlyList<string>? Cast = null);
