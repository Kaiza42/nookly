using Nookly.Domain.Media;

namespace Nookly.Application.Media;

public sealed record UpdateMediaRequest(
    string Title,
    MediaType Type,
    string? Description,
    MediaStatus Status,
    decimal? PersonalRating);
