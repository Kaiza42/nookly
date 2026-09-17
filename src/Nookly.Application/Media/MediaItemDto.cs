using Nookly.Domain.Media;

namespace Nookly.Application.Media;

public sealed record MediaItemDto(
    Guid Id,
    string Title,
    string? Description,
    MediaType Type,
    MediaStatus Status,
    decimal? PersonalRating,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
