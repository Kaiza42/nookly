namespace Nookly.Application.Media;

public sealed record UpdateMediaRequest(
    Nookly.Domain.Media.MediaStatus Status,
    decimal? PersonalRating,
    string? PersonalNotes);
