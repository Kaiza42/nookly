namespace Nookly.Contracts.Media;

public sealed record MediaItemResponse(
    Guid Id,
    string Title,
    string? Description,
    MediaType Type,
    MediaStatus Status,
    decimal? PersonalRating,
    string? ExternalSource,
    string? ExternalId,
    string? PosterUrl,
    decimal? CommunityRating,
    DateOnly? ReleaseDate,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
