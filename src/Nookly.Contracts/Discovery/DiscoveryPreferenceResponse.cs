using Nookly.Contracts.Media;

namespace Nookly.Contracts.Discovery;

public sealed record DiscoveryPreferenceResponse(
    string ExternalSource,
    string ExternalId,
    string Title,
    MediaType? Type,
    bool IsLiked,
    string? Description = null,
    string? PosterUrl = null,
    decimal? CommunityRating = null,
    DateOnly? ReleaseDate = null,
    IReadOnlyList<string>? Cast = null);
