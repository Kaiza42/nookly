using CommunityToolkit.Mvvm.ComponentModel;
using Nookly.Contracts.Details;
using Nookly.Contracts.Media;

namespace Nookly.Desktop.ViewModels;

public sealed class MediaDetailsViewModel(MediaDetailsResponse details)
{
    public MediaDetailsResponse Details => details;
    public string ExternalId => details.ExternalId;
    public MediaType Type => details.Type;
    public string Title => details.Title;
    public string? Description => details.Description;
    public string? PosterUrl => details.PosterUrl;
    public string ReleaseDateLabel => details.ReleaseDate?.ToString("dd/MM/yyyy") ?? "Date inconnue";
    public string StatusLabel => details.AirStatus ?? "Statut inconnu";
    public string RuntimeLabel => details.RuntimeMinutes is null ? "Duree inconnue" : $"{details.RuntimeMinutes} min";
    public string RatingLabel => details.CommunityRating is null ? "Non note" : $"{details.CommunityRating:0.0}/10";
    public string GenresLabel => details.Genres.Count == 0 ? "Genres non disponibles" : string.Join(" · ", details.Genres);
    public string DirectorsLabel => details.Directors.Count == 0 ? "Realisation non disponible" : string.Join(", ", details.Directors);
    public string CastLabel => details.Cast.Count == 0 ? "Distribution non disponible" : string.Join(", ", details.Cast);
    public string? TrailerUrl => details.TrailerUrl;
    public bool HasTrailer => TrailerUrl is not null;
    public bool HasSeasons => details.Seasons.Count > 0;
    public IReadOnlyList<SeasonSummaryViewModel> Seasons => details.Seasons.Select(item => new SeasonSummaryViewModel(item)).ToArray();
}

public sealed class SeasonSummaryViewModel(SeasonSummaryResponse season)
{
    public int Number => season.Number;
    public string Title => season.Title;
    public string? Description => season.Description;
    public string? PosterUrl => season.PosterUrl;
    public string MetaLabel => $"{season.EpisodeCount} episodes · {(season.CommunityRating is null ? "Non notee" : $"{season.CommunityRating:0.0}/10")}";
    public override string ToString() => $"Saison {Number}";
}

public sealed partial class SeasonDetailsViewModel : ObservableObject
{
    private readonly SeasonDetailsResponse season;

    public SeasonDetailsViewModel(SeasonDetailsResponse season, SeasonProgressResponse progress)
    {
        this.season = season;
        personalNotes = progress.PersonalNotes ?? string.Empty;
        watchedEpisodes = progress.WatchedEpisodes;
        Episodes = season.Episodes.Select(item =>
        {
            var saved = progress.Episodes.FirstOrDefault(value => value.EpisodeNumber == item.Number);
            return new EpisodeViewModel(item, saved);
        }).ToArray();
    }

    [ObservableProperty]
    private string personalNotes;

    [ObservableProperty]
    private int watchedEpisodes;

    public string Title => season.Title;
    public int Number => season.Number;
    public string? Description => season.Description;
    public string? PosterUrl => season.PosterUrl;
    public string MetaLabel => $"{season.Episodes.Count} episodes · {(season.CommunityRating is null ? "Non notee" : $"{season.CommunityRating:0.0}/10")}";
    public string ProgressLabel => $"{WatchedEpisodes} / {season.Episodes.Count} episodes vus";
    public IReadOnlyList<EpisodeViewModel> Episodes { get; }

    partial void OnWatchedEpisodesChanged(int value) => OnPropertyChanged(nameof(ProgressLabel));

    public void RefreshWatchedCount() => WatchedEpisodes = Episodes.Count(item => item.IsWatched);
}

public sealed partial class EpisodeViewModel : ObservableObject
{
    private readonly EpisodeResponse episode;

    public EpisodeViewModel(EpisodeResponse episode, EpisodeProgressResponse? progress)
    {
        this.episode = episode;
        isWatched = progress?.IsWatched == true;
        personalNotes = progress?.PersonalNotes ?? string.Empty;
    }

    [ObservableProperty]
    private bool isWatched;

    [ObservableProperty]
    private string personalNotes;

    public int Number => episode.Number;
    public string NumberLabel => $"Episode {episode.Number}";
    public string Title => episode.Title;
    public string? Description => episode.Description;
    public string? ImageUrl => episode.ImageUrl;
    public string MetaLabel => string.Join(" · ", new[]
    {
        episode.AirDate?.ToString("dd/MM/yyyy"),
        episode.RuntimeMinutes is null ? null : $"{episode.RuntimeMinutes} min",
        episode.CommunityRating is null ? "Non note" : $"{episode.CommunityRating:0.0}/10"
    }.Where(value => value is not null));
}
