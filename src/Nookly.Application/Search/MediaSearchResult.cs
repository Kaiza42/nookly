using Nookly.Domain.Media;

namespace Nookly.Application.Search;

public sealed record MediaSearchResult(
    string ExternalSource,
    string ExternalId,
    string Title,
    string? Description,
    MediaType Type,
    string? PosterUrl,
    decimal? CommunityRating,
    DateOnly? ReleaseDate);
