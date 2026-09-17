namespace Nookly.Contracts.Media;

public sealed record MediaItemResponse(
    Guid Id,
    string Title,
    string? Description,
    MediaType Type,
    MediaStatus Status,
    decimal? PersonalRating,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
