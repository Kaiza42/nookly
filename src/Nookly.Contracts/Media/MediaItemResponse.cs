namespace Nookly.Contracts.Media;

public sealed record MediaItemResponse(
    Guid Id,
    string Title,
    string? Description,
    MediaType Type,
    MediaStatus Status,
    decimal? PersonalRating,
    string? PersonalNotes,
    bool IsFavorite,
    int? CurrentSeason,
    int? CurrentEpisode,
    string? ExternalSource,
    string? ExternalId,
    string? PosterUrl,
    decimal? CommunityRating,
    DateOnly? ReleaseDate,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
