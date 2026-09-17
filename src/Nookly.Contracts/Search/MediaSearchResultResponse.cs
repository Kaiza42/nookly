using Nookly.Contracts.Media;

namespace Nookly.Contracts.Search;

public sealed record MediaSearchResultResponse(
    string ExternalSource,
    string ExternalId,
    string Title,
    string? Description,
    MediaType Type,
    string? PosterUrl,
    decimal? CommunityRating,
    DateOnly? ReleaseDate);
