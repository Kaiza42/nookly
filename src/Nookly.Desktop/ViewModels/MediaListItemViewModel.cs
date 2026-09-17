using Nookly.Contracts.Media;

namespace Nookly.Desktop.ViewModels;

public sealed class MediaListItemViewModel(MediaItemResponse mediaItem)
{
    public Guid Id => mediaItem.Id;
    public string Title => mediaItem.Title;
    public string? Description => mediaItem.Description;
    public MediaType Type => mediaItem.Type;
    public MediaStatus Status => mediaItem.Status;
    public decimal? PersonalRating => mediaItem.PersonalRating;
    public string? PersonalNotes => mediaItem.PersonalNotes;
    public string? PosterUrl => mediaItem.PosterUrl;
    public bool IsFavorite => mediaItem.IsFavorite;
    public int? CurrentSeason => mediaItem.CurrentSeason;
    public int? CurrentEpisode => mediaItem.CurrentEpisode;
    public decimal? CommunityRating => mediaItem.CommunityRating;
    public DateOnly? ReleaseDate => mediaItem.ReleaseDate;
    public string? ExternalSource => mediaItem.ExternalSource;
    public string? ExternalId => mediaItem.ExternalId;
    public bool SupportsEpisodeProgress => Type is MediaType.TvSeries or MediaType.Anime;

    public string TypeLabel => Type switch
    {
        MediaType.Movie => "Film",
        MediaType.TvSeries => "Serie",
        MediaType.Anime => "Anime",
        MediaType.Manga => "Manga",
        _ => Type.ToString()
    };

    public string StatusLabel => Status switch
    {
        MediaStatus.Planned => "A decouvrir",
        MediaStatus.InProgress => "En cours",
        MediaStatus.Completed => "Termine",
        MediaStatus.OnHold => "En pause",
        MediaStatus.Dropped => "Abandonne",
        _ => Status.ToString()
    };

    public string RatingLabel => PersonalRating is null
        ? "Non note"
        : $"{PersonalRating:0.#}/10";

    public string AddedDateLabel => mediaItem.CreatedAtUtc.LocalDateTime.ToString("dd/MM/yyyy");
    public string ReleaseDateLabel => ReleaseDate?.ToString("dd/MM/yyyy") ?? "Date inconnue";
    public string CommunityRatingLabel => CommunityRating is null
        ? "Non noté"
        : $"{CommunityRating:0.0}/10";
    public string ProgressLabel => SupportsEpisodeProgress && (CurrentSeason is not null || CurrentEpisode is not null)
        ? $"Saison {CurrentSeason ?? 0}, épisode {CurrentEpisode ?? 0}"
        : "Progression non renseignée";
}
