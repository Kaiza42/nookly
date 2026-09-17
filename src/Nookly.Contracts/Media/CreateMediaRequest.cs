namespace Nookly.Contracts.Media;

public sealed record CreateMediaRequest(
    string Title,
    MediaType Type,
    string? Description,
    string? ExternalSource = null,
    string? ExternalId = null,
    string? PosterUrl = null,
    decimal? CommunityRating = null,
    DateOnly? ReleaseDate = null);
