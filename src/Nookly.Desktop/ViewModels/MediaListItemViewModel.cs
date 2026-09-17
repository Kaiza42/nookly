using Nookly.Contracts.Media;

namespace Nookly.Desktop.ViewModels;

public sealed class MediaListItemViewModel(MediaItemResponse mediaItem)
{
    public string Title => mediaItem.Title;
    public string? Description => mediaItem.Description;
    public string TypeLabel => mediaItem.Type switch
    {
        MediaType.Movie => "Film",
        MediaType.TvSeries => "Serie",
        MediaType.Anime => "Anime",
        MediaType.Manga => "Manga",
        _ => mediaItem.Type.ToString()
    };

    public string StatusLabel => mediaItem.Status switch
    {
        MediaStatus.Planned => "A decouvrir",
        MediaStatus.InProgress => "En cours",
        MediaStatus.Completed => "Termine",
        MediaStatus.OnHold => "En pause",
        MediaStatus.Dropped => "Abandonne",
        _ => mediaItem.Status.ToString()
    };

    public string AddedDateLabel => mediaItem.CreatedAtUtc.LocalDateTime.ToString("dd/MM/yyyy");
}
