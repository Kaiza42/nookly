namespace Nookly.Contracts.Media;

public sealed record UpdateMediaRequest(
    MediaStatus Status,
    decimal? PersonalRating,
    string? PersonalNotes,
    bool IsFavorite,
    int? CurrentSeason,
    int? CurrentEpisode);
